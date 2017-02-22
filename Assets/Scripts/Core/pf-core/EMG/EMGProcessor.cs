using System;
using System.Collections.Generic;
using System.Threading;
using System.Numerics;

using Accord.Math;
using System.IO;

namespace pfcore
{
	public class EMGProcessor
	{
		public enum Mode
		{
			IDLE,
			TRAINING,
			PREDICTING,
			WRITING
		}

		private EMGReader reader;

		public const int FFT_SAMPLE_SIZE = 128;
		public const double FREQ_STEP = (EMGPacket.SAMPLE_RATE) / FFT_SAMPLE_SIZE;
		public const int SKIPS_AFTER_TRANSITION = 3;
		public const int FEATURE_COUNT = 2;
		public const float LOWER_FREQ = 50;
		public const float HIGHER_FREQ = 150;

		private Trainer trainer;
		private List<TrainingValue> trainingData;
		private MuscleState currentMuscleState = MuscleState.NONE;
		public MuscleState CurrentMuscleState
		{
			set
			{
				currentMuscleState = value;
				skipsRemaining = SKIPS_AFTER_TRANSITION;
			}
			get
			{
				return currentMuscleState;
			}
		}

		private FileStream outFileStream = null;
		public FileStream OutFileStream
		{
			set
			{
				outFileStream = value;
			}
		}

		public int TrainingDataLength
		{
			get
			{
				return trainingData.Count;
			}
		}

		private MuscleState predictedMuscleState = MuscleState.NONE;
		public MuscleState PredictedMuscleState
		{
			get
			{
				return predictedMuscleState;
			}
		}

		private Thread readerThread;
		private Mode mode = Mode.IDLE;
		public Mode CurrentMode
		{
			get
			{
				return mode;
			}
		}

		private int skipsRemaining = 0;

		private List<EMGPacket> rawReadings = new List<EMGPacket>();
		private List<EMGReading> readings = new List<EMGReading>();
		public List<EMGReading> Readings
		{
			get
			{
				return readings;
			}
		}

		private List<Complex> fftResults = new List<Complex>();
		public List<Complex> FFTResults
		{
			get
			{
				return fftResults;
			}
		}

		private List<Action> processorCallbacks = new List<Action>();

		public EMGProcessor(EMGReader reader)
		{
			this.reader = reader;

			trainer = new Trainer(FEATURE_COUNT, ClassifierType.DecisionTree);
			trainingData = new List<TrainingValue>();
		}

		public void Start()
		{
			readerThread = new Thread(new ThreadStart(reader.Start));
			readerThread.Start();
		}

		public void StopAndJoin()
		{
			reader.Stop();
			readerThread.Join();
		}

		public void AddProcessorCallback(Action callback)
		{
			processorCallbacks.Add(callback);
		}

		public void ChangeMode(Mode newMode)
		{
			if (mode == Mode.TRAINING)
			{
				EndTraining();
			}
			else if (mode == Mode.PREDICTING)
			{
				predictedMuscleState = MuscleState.NONE;
                trainer = new Trainer(FEATURE_COUNT, ClassifierType.DecisionTree);
			}
			else if (mode == Mode.WRITING)
			{
				outFileStream = null;
			}

			mode = newMode;
		}

		public void Update()
		{
			EMGPacket packet;
			MuscleState hintedMuscleState = MuscleState.NONE;

			while (reader.TryDequeue(out packet))
			{
				hintedMuscleState = packet.muscleStateHint;

				packet.muscleStateHint = currentMuscleState;
				rawReadings.Add(packet);
			}

			if (hintedMuscleState != MuscleState.NONE)
			{
				// If packages are being read from file, a MuscleState hint will be stored in them
				// Take the current muscle state from packets instead of being specified by the player
				currentMuscleState = hintedMuscleState;
			}

			if (rawReadings.Count >= FFT_SAMPLE_SIZE)
			{
				Idle();

				switch (mode)
				{
					case Mode.TRAINING:
						Train();
						break;
					case Mode.PREDICTING:
						Predict();
						break;
					case Mode.WRITING:
						Write();
						break;
					default:
						break;
				}

				processorCallbacks.ForEach(callback => callback());

				rawReadings.Clear();
				readings.Clear();
			}

			if (mode != Mode.WRITING)
			{
				/* Discard packets received during processing */
				reader.ClearQueue();
			}
		}

		private void Train()
		{
			if (skipsRemaining > 0)
			{
				skipsRemaining--;
				return;
			}

			TrainingValue value = new TrainingValue((int)currentMuscleState, FEATURE_COUNT);
			FillTrainingValue(ref value, fftResults);
			trainingData.Add(value);
		}

		public static void FillTrainingValue(ref TrainingValue value, List<Complex> fftResults)
		{
			float freqRange = HIGHER_FREQ - LOWER_FREQ;
			float freqStep = freqRange / FEATURE_COUNT;
			float lower = LOWER_FREQ;
			float higher = lower + freqStep;

			for (int i = 0; i < FEATURE_COUNT; i++)
			{
				value.Features[i] = PSD(fftResults, EMGPacket.SAMPLE_RATE, lower, higher);
				lower += freqStep;
				higher += freqStep;
			}
		}

		public static double PSD(List<Complex> fftResults, float sampleFreq, float freqLow, float freqHigh)
		{
			float freqStep = sampleFreq / fftResults.Count;
			int halfSize = fftResults.Count / 2;

			int startIndex = (int)(freqLow / freqStep);
			int stopIndex = (int)(freqHigh / freqStep) + 1;
			stopIndex = Math.Min(stopIndex, halfSize);

			double result = 0;

			for (int i = startIndex; i < stopIndex; i++)
			{
				result += fftResults[i].Magnitude;
			}

			return result;
		}

		private void EndTraining()
		{
            if (trainingData.Count > 0) {
                Train(trainingData, trainer);
            }

            trainingData.Clear();
		}

		internal static void Train(List<TrainingValue> trainingData, Trainer trainer)
		{
			trainer.Train(trainingData);
		}

		private void Predict()
		{
			TrainingValue tmp = new TrainingValue();
			tmp.Features = new double[FEATURE_COUNT];
			FillTrainingValue(ref tmp, fftResults);

			int result = trainer.Predict(tmp);
			predictedMuscleState = (MuscleState)result;
		}

		public static float ValueFromPacket(EMGPacket packet)
		{
			return packet.channels[0];
		}

		private void Idle()
		{
			readings.Clear();
			readings.Capacity = rawReadings.Count;

			foreach (EMGPacket packet in rawReadings)
			{
				readings.Add(new EMGReading(ValueFromPacket(packet), packet.timeStamp));
			}

			Complex[] data = new Complex[FFT_SAMPLE_SIZE];
			for (int i = 0; i < FFT_SAMPLE_SIZE; i++)
			{
				data[i] = new Complex(ValueFromPacket(rawReadings[i]), 0);
			}

			FourierTransform.FFT(data, FourierTransform.Direction.Forward);

			fftResults.Clear();
			fftResults.Capacity = data.Length;
			fftResults.AddRange(data);
		}

		private void Write()
		{
			if (outFileStream == null)
			{
				throw new Exception("WRITE mode: outFileStream is null");
			}

			byte[] buffer = new byte[EMGPacket.PACKET_SIZE_W_HINT];
			foreach (EMGPacket packet in rawReadings)
			{
				packet.Pack(buffer);
				outFileStream.Write(buffer, 0, buffer.Length);
			}
		}
	}
}
