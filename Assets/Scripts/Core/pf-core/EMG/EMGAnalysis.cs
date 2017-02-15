using Accord.MachineLearning.DecisionTrees;
using Accord.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading;

namespace pfcore
{
	class EMGAnalysis
	{
		private string filename;
		private const float TRAINING_COUNT_PCTG = 0.75f;

		public EMGAnalysis(string filename)
		{
			this.filename = filename;
		}

		public void PrintResults(bool writeCSV)
		{
			Console.WriteLine("--------------------------------");
			Console.WriteLine("Using file: " + filename);
			List<EMGPacket> packets = ReadPackets(filename);

			Console.WriteLine("Read: " + packets.Count + " packets.");
			int toDiscard = packets.Count % EMGProcessor.FFT_SAMPLE_SIZE;
			Console.WriteLine("Will discard: " + toDiscard + " packets.");
			Console.WriteLine("FFT Samples size is: " + EMGProcessor.FFT_SAMPLE_SIZE);

			packets.RemoveRange(packets.Count - toDiscard, toDiscard);

			Console.WriteLine("--------------------------------");
			Console.WriteLine("New packet count: " + packets.Count);
			Console.WriteLine("(" + (packets.Count / EMGPacket.SAMPLE_RATE) + " seconds of data)");
			Console.WriteLine("(" + (packets.Count / EMGProcessor.FFT_SAMPLE_SIZE) + " training values)");

			foreach (EMGPacket packet in packets)
			{
				if (packet.muscleStateHint == MuscleState.NONE)
				{
					Console.WriteLine("ERROR: One or more packets are missing a MuscleState hint.");
					return;
				}
			}

			Console.WriteLine("--------------------------------");

			int trainingCount = (int)(TRAINING_COUNT_PCTG * packets.Count);
			trainingCount -= trainingCount % EMGProcessor.FFT_SAMPLE_SIZE;

			int predictionCount = packets.Count - trainingCount;

			Console.WriteLine("Training packets count: " + trainingCount);
			Console.WriteLine("Prediction packets count: " + predictionCount);

			List<EMGPacket> trainingPackets = packets.GetRange(0, trainingCount);
			List<EMGPacket> predictionPackets = packets.GetRange(packets.Count - predictionCount, predictionCount);

			List<TrainingValue> trainingValues = GetTrainingValues(trainingPackets, true);
			List<TrainingValue> predictionValues = GetTrainingValues(predictionPackets, false);

			if (writeCSV)
			{
				System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
				customCulture.NumberFormat.NumberDecimalSeparator = ".";
				Thread.CurrentThread.CurrentCulture = customCulture;

				StringBuilder sb = new StringBuilder();

				foreach (TrainingValue val in trainingValues)
				{
					sb.AppendLine(csvLineFromTrainingValue(val));
				}

				foreach (TrainingValue val in predictionValues)
				{
					sb.AppendLine(csvLineFromTrainingValue(val));
				}

				File.WriteAllText(filename + ".csv", sb.ToString());
			}

			Console.WriteLine("Training values count: " + trainingValues.Count);
			Console.WriteLine("Prediction values count: " + predictionValues.Count);

			Console.WriteLine("--------------------------------");

			Trainer trainer = new Trainer(EMGProcessor.FEATURE_COUNT, ClassifierType.DecisionTree);
			EMGProcessor.Train(trainingValues, trainer);

			int[,] confMat = new int[2, 2];

			foreach (TrainingValue predValue in predictionValues)
			{
				int result = trainer.Predict(predValue);
				MuscleState muscleState = (MuscleState)result;

				int i = (predValue.State == (int)MuscleState.TENSE) ? 1 : 0;
				int j = (muscleState == MuscleState.TENSE) ? 1 : 0;

				confMat[i, j]++;
			}

			Console.WriteLine("Confusion matrix:");
			Console.WriteLine("   R     T");
			Console.WriteLine("R  {0}    {1}", confMat[0, 0], confMat[0, 1]);
			Console.WriteLine("T  {0}    {1}", confMat[1, 0], confMat[1, 1]);

			double sensitivity = (double)confMat[0, 0] / (confMat[0, 0] + confMat[0, 1]);
			double specificity = (double)confMat[1, 1] / (confMat[1, 0] + confMat[1, 1]);
			double accuracy = confMat[0, 0] + confMat[1, 1];
			accuracy /= predictionValues.Count;

			Console.WriteLine("Sensitivity: " + sensitivity);
			Console.WriteLine("Specificity: " + specificity);
			Console.WriteLine("Accuracy: " + accuracy);
		}

		private string csvLineFromTrainingValue(TrainingValue val)
		{
			return string.Format("{0},{1},{2}", val.Features[0], val.Features[1], (int)val.State);
		}

		internal static List<TrainingValue> GetTrainingValues(List<EMGPacket> packets, bool enableSkip)
		{
			List<TrainingValue> values = new List<TrainingValue>(EMGProcessor.FFT_SAMPLE_SIZE);

			int skipsRemaining = 0;

			for (int i = 0; i < packets.Count / EMGProcessor.FFT_SAMPLE_SIZE; i++)
			{
				if (enableSkip && skipsRemaining > 0)
				{
					skipsRemaining--;
					continue;
				}

				Complex[] data = new Complex[EMGProcessor.FFT_SAMPLE_SIZE];
				int start = i * EMGProcessor.FFT_SAMPLE_SIZE;
				int end = start + EMGProcessor.FFT_SAMPLE_SIZE;
				MuscleState startMuscleState = packets[start].muscleStateHint;

				for (int j = start; j < end; j++)
				{
					EMGPacket packet = packets[j];
					if (packet.muscleStateHint != startMuscleState)
					{
						skipsRemaining += EMGProcessor.SKIPS_AFTER_TRANSITION;
						break;
					}

					data[j - start] = new Complex(EMGProcessor.ValueFromPacket(packet), 0);
				}

				if (enableSkip && skipsRemaining > 0)
				{
					continue;
				}

				FourierTransform.FFT(data, FourierTransform.Direction.Forward);
				List<Complex> fftResults = new List<Complex>(data);

				TrainingValue trainingValue = new TrainingValue((int)packets[start].muscleStateHint, EMGProcessor.FEATURE_COUNT);

				EMGProcessor.FillTrainingValue(ref trainingValue, fftResults);
				values.Add(trainingValue);
			}

			return values;
		}

		internal static List<EMGPacket> ReadPackets(String filename)
		{
			FileStream fileStream = File.OpenRead(filename);
			List<EMGPacket> packets = new List<EMGPacket>();

			EMGFileReader reader = new EMGFileReader(fileStream, -1);
			Thread readerThread = new Thread(new ThreadStart(reader.Start));

			reader.DisableSerialDelay();
			readerThread.Start();

			EMGPacket packet;
			readerThread.Join();

			while (reader.TryDequeue(out packet))
			{
				packets.Add(packet);
			}

			return packets;
		}
	}
}
