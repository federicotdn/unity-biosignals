using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.IO;

namespace pfcore
{
	public class EEGSessionRecorder
	{
		private EEGProcessor processor;
		private EEGData trainingData;
		private EEGData predictionData;


		public EEGSessionRecorder(EEGProcessor processor)
		{
			this.processor = processor;
		}

		public EEGSessionRecorder(EEGProcessor processor, string trainingPath, string predicitonPath)
		{
			this.processor = processor;

			List<string> lines = ReadCSV(trainingPath);

			trainingData = getData(lines);

			lines = ReadCSV(predicitonPath);

			predictionData = getData(lines);

		}

		public void Start()
		{
			if (trainingData.features != null && predictionData.features != null)
			{
				processor.Train(trainingData);

				int[] answers = processor.Predict(predictionData.features);

				int[,] confusionMatrix = new int[2, 2];
				for (int i = 0; i < answers.Length; i++)
				{
					if (predictionData.outputs[i] == (int)TrainingMode.EYES_CLOSED)
					{
						if (answers[i] == (int)TrainingMode.EYES_CLOSED)
						{
							confusionMatrix[0, 0]++;
						}
						else
						{
							confusionMatrix[0, 1]++;
						}
					}
					else
					{
						if (answers[i] == (int)TrainingMode.EYES_OPENED)
						{
							confusionMatrix[1, 1]++;
						}
						else
						{
							confusionMatrix[1, 0]++;
						}
					}
				}

				Console.WriteLine("Finished training and predicting.");
				Console.WriteLine("\nConfusion matrix: \n");
				Console.WriteLine("  C     O");

				int truePositive = confusionMatrix[0, 0];
				int falseNegative = confusionMatrix[0, 1];
				int trueNegative = confusionMatrix[1, 1];
				int falsePositive = confusionMatrix[1, 0];

				float SE = truePositive / (float)(truePositive + falseNegative);
				float SP = trueNegative / (float)(trueNegative + falsePositive);

				Console.WriteLine("C " + confusionMatrix[0, 0] + "   " + confusionMatrix[0, 1]);
				Console.WriteLine("O " + confusionMatrix[1, 0] + "   " + confusionMatrix[1, 1]);
				Console.WriteLine("\nAAC: " + (confusionMatrix[0, 0] + confusionMatrix[1, 1]) / (float)(predictionData.outputs.Length));

				Console.WriteLine("Sensitivity: " + SE);
				Console.WriteLine("Specificity: " + SP);
			}
			else
			{
				Thread readLineThread = new Thread(new ThreadStart(readInput));
				readLineThread.Start();


				processor.Start();
				while (true)
				{
					processor.Update();
				}
			}
		}

		private void readInput()
		{
			while (true)
			{
				ConsoleKeyInfo k = Console.ReadKey();
				if (k.Key == ConsoleKey.Spacebar)
				{
					if (!processor.Training)
					{
						processor.Training = true;
					}

					if (processor.Mode == TrainingMode.EYES_CLOSED)
					{
						processor.Mode = TrainingMode.EYES_OPENED;
					}
					else
					{
						processor.Mode = TrainingMode.EYES_CLOSED;
					}
				}
				else if (k.Key == ConsoleKey.F)
				{
					processor.Training = false;
					SaveToCSV();
					Environment.Exit(0);
				}
			}
		}


		private void SaveToCSV()
		{
			StringBuilder csv = new StringBuilder();

			List<TrainingMode> modes = processor.Modes;

			string baseFilePath = Directory.GetCurrentDirectory();
			baseFilePath += "/" + string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now);

			csv.AppendLine("Alpha, Beta, Eyes");

			int size = Math.Min(processor.Alpha.Count, processor.Beta.Count);
			for (int i = 0; i < size; i++)
			{
				string newLine = string.Format("{0},{1},{2}", processor.Alpha[i], processor.Beta[i], (int)modes[i]);
				csv.AppendLine(newLine);
			}


			string bandPowersPath = baseFilePath + "-band-powers.csv";
			Console.WriteLine("Saving band powers to: " + bandPowersPath);
			File.WriteAllText(bandPowersPath, csv.ToString());

			csv = new StringBuilder();

			csv.AppendLine("EEG1, EEG2, EEG3, EEG4, EEG5, Eyes");

			for (int i = 0; i < processor.RawModes.Count; i++)
			{
				int rawIndex = i * 5;
				string newLine = string.Format("{0},{1},{2},{3},{4},{5}", processor.RawEEG[rawIndex], processor.RawEEG[rawIndex + 1],
											   processor.RawEEG[rawIndex + 2], processor.RawEEG[rawIndex + 3],
											   processor.RawEEG[rawIndex + 4], (int)processor.RawModes[i]);
				csv.AppendLine(newLine);
			}

			string rawPath = baseFilePath + "-raw.csv";
			Console.WriteLine("Saving raw EEG to: " + rawPath);
			File.WriteAllText(rawPath, csv.ToString());
		}

		private List<string> ReadCSV(string path)
		{
			StreamReader r = new StreamReader(File.OpenRead(path));
			List<string> lines = new List<string>();
			while (!r.EndOfStream)
			{
				var line = r.ReadLine();
				lines.Add(line);
			}
			r.Close();
			float aux;
			float.TryParse(lines[0].Split(',')[0], out aux);
			if (Math.Abs(aux) < 0.00001)
			{
				lines.RemoveAt(0);
			}
			return lines;
		}

		private EEGData getData(List<string> lines)
		{
			lines.RemoveAt(0);
			double[][] features = new double[lines.Count][];
			int[] outputs = new int[lines.Count];
			int i = 0;

			foreach (string line in lines)
			{
				string[] values = line.Split(',');
				features[i] = new double[2];
				features[i][0] = double.Parse(values[0]);
				features[i][1] = double.Parse(values[1]);
				outputs[i] = int.Parse(values[2]);
				i++;
			}

			return new EEGData(features, outputs);
		}
	}
}
