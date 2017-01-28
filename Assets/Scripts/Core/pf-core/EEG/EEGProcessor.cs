using System;
using System.Threading;
using System.Collections.Generic;
using Accord.MachineLearning.DecisionTrees.Learning;
using Accord.MachineLearning.DecisionTrees;

using System.Numerics;

using Accord.Math;

namespace pfcore
{
	public enum TrainingMode
	{
		EYES_OPENED = 1, EYES_CLOSED = 0, IDLE = 2
	}

	public struct EEGReading
	{
		public float value;
		public long timeStamp;

		public EEGReading(float value, long timeStamp)
		{
			this.value = value;
			this.timeStamp = timeStamp;
		}
	}


	public struct EEGData
	{
		public double[][] features;
		public int[] outputs;

		public EEGData(double[][] features, int[] outputs)
		{
			this.features = features;
			this.outputs = outputs;
		}
	}

	public class EEGProcessor
	{
		public List<float> Alpha { get; private set; }
		public List<float> Beta { get; private set; }
		public List<float> RawEEG { get; private set; }
		public List<TrainingMode> Modes { get; private set; }
		public List<TrainingMode> RawModes { get; private set; }
		public List<Complex> FFTResults { get; private set; }

		public List<EEGReading> AlphaReadings { get; private set; }
		private List<EEGPacket> AlphaPackets = new List<EEGPacket>();

		public const int FFT_SAMPLE_SIZE = 256;
		public const double FREQ_STEP = EMGPacket.SAMPLE_RATE / FFT_SAMPLE_SIZE;

		private List<float> readingsMean = new List<float>();

		public Action ProcessorCallback;

		EEGReader reader;
		DecisionTree tree;

		TrainingMode mode;
		public TrainingMode Mode
		{
			get
			{
				return mode;
			}

			set
			{
				mode = value;

				switch (mode)
				{
					case TrainingMode.EYES_CLOSED:
						Console.WriteLine("Switched to eyes CLOSED");
						break;
					case TrainingMode.EYES_OPENED:
						Console.WriteLine("Switched to eyes OPENED");
						break;
					case TrainingMode.IDLE:
						Console.WriteLine("Switched to IDLE");
						break;
				}
			}
		}

		public bool Online { get; set; }
		private bool training;
		public bool Training
		{
			get
			{
				return training;
			}

			set
			{
				training = value;
				if (training)
				{
					Console.WriteLine("Started Training");
				}
				else
				{
					Console.WriteLine("Finished Training");
				}
			}
		}

		public EEGProcessor(EEGReader reader, bool online)
		{
			Modes = new List<TrainingMode>();
			RawModes = new List<TrainingMode>();
			AlphaReadings = new List<EEGReading>();
			Alpha = new List<float>();
			Beta = new List<float>();
			RawEEG = new List<float>();
			FFTResults = new List<Complex>();

			this.reader = reader;
			Online = online;
			Mode = TrainingMode.IDLE;

		}

		public EEGProcessor(EEGReader reader) : this(reader, false)
		{
		}

		public void Start()
		{
			if (Online)
			{
				Thread readerThread = new Thread(new ThreadStart(reader.Start));
				readerThread.Start();
			}
		}

		public void Update()
		{
			ConcurrentQueue<EEGPacket> q = reader.PacketQueue;
			EEGPacket packet;

			while (q.TryDequeue(out packet))
			{
				if (Training)
				{
					if (true)
					{
						switch (packet.Type)
						{
							case DataType.ALPHA:
								Alpha.Add(packet.Data[0]);
								AlphaPackets.Add(packet);
								Modes.Add(mode);
								break;
							case DataType.BETA:
								Beta.Add(packet.Data[0]);
								break;
							case DataType.RAW:
								RawEEG.AddRange(packet.Data);
								RawModes.Add(mode);
								break;

						}

						if (RawEEG.Count / 4 > FFT_SAMPLE_SIZE)
						{
							if (ProcessorCallback != null)
							{
								RunFFT();
								ProcessorCallback();
								AlphaPackets.Clear();
								RawEEG.Clear();
							}

						}
					}

				}

			}
		}

		public void Train(EEGData eegData)
		{

			tree = new DecisionTree(
				inputs: new List<DecisionVariable>
					{
						DecisionVariable.Continuous("X"),
						DecisionVariable.Continuous("Y")
					},
				classes: 2);

			C45Learning teacher = new C45Learning(tree);
			teacher.Learn(eegData.features, eegData.outputs);

			//var learner = new NaiveBayesLearning<NormalDistribution>();

			//var nb = learner.Learn(features, outputs);
			//return nb;
		}

		public int[] Predict(double[][] features)
		{
			if (tree == null)
			{
				throw new Exception("Train must be called first!");
			}
			return tree.Decide(features);
		}

		private void RunFFT()
		{
			AlphaReadings.Clear();
			AlphaReadings.Capacity = AlphaPackets.Count;
			foreach (EEGPacket packet in AlphaPackets)
			{
				AlphaReadings.Add(new EEGReading(packet.Data[0], packet.timestamp));
			}


			readingsMean.Clear();

			for (int i = 0; i + 3 < RawEEG.Count; i += 4)
			{
				readingsMean.Add((RawEEG[i] + RawEEG[i + 1] + RawEEG[i + 2] + RawEEG[i + 3]) / 4);
			}

			Complex[] data = new Complex[FFT_SAMPLE_SIZE];
			for (int i = 0; i < FFT_SAMPLE_SIZE; i++)
			{
				data[i] = new Complex(readingsMean[i], 0);
			}

			FourierTransform.FFT(data, FourierTransform.Direction.Forward);

			FFTResults.Clear();
			FFTResults.Capacity = data.Length;
			FFTResults.AddRange(data);
		}

	}
}
