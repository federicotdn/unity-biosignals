using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Collections.Generic;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.DecisionTrees.Learning;
using Accord.MachineLearning.DecisionTrees;
using Accord.MachineLearning.Bayes;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics.Distributions.Univariate;
using System.IO;
using System.Text;

namespace pfcore
{
	public enum TrainingMode
	{
		EYES_OPENED = 1, EYES_CLOSED = 0, IDLE = 2
	}

	public class EEGProcessor
	{
		private EEGReader reader;
		private List<float> data = new List<float>();
		private List<TrainingMode> modes = new List<TrainingMode>();
		private int currentCount;
		private SupportVectorMachine svm;
		private string trainingCSVPath;
		private string predictionCSVPath;

		private const int IGNORE_COUNT = 25;

		private TrainingMode mode;
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
				currentCount = 0;

				int i = 0;
				while (i < IGNORE_COUNT && data.Count > 0 && i < data.Count)
				{
					data.RemoveAt(i);
					modes.RemoveAt(i);
					i++;
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
					currentCount = 0;
					Console.WriteLine("Started Training");
				}
				else
				{
					if (Online)
					{
						Train();
					}
					else
					{
						SaveToCSV();
					}

					Console.WriteLine("Finished Training");
				}
			}
		}

		public EEGProcessor(EEGReader reader, bool online)
		{
			this.reader = reader;
			Online = online;
			Mode = TrainingMode.IDLE;
		}

		public EEGProcessor(EEGReader reader) : this(reader, false)
		{
		}

		public EEGProcessor(EEGReader reader, string trainingPath, string preditctionPath) : this(reader, false)
		{
			trainingCSVPath = trainingPath;
			predictionCSVPath = preditctionPath;
		}

		public void Start()
		{
			if (!Online && trainingCSVPath != null && predictionCSVPath != null)
			{
				List<string> lines = ReadCSV(trainingCSVPath);

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

				var tree = Train(features, outputs);

				lines = ReadCSV(predictionCSVPath);

				features = new double[lines.Count][];
				int[] expectedOutputs = new int[lines.Count];
				i = 0;
				foreach (string line in lines)
				{
					string[] values = line.Split(',');
					features[i] = new double[2];
					features[i][0] = double.Parse(values[0]);
					features[i][1] = double.Parse(values[1]);
					expectedOutputs[i] = int.Parse(values[2]);
					i++;
				}

				int[] answers = tree.Decide(features);

				int[,] confusionMatrix = new int[2, 2];
				for (i = 0; i < answers.Length; i++)
				{
					if (expectedOutputs[i] == (int)TrainingMode.EYES_CLOSED)
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
				Console.WriteLine("\nAAC: " + (confusionMatrix[0, 0] + confusionMatrix[1, 1]) / (float)(expectedOutputs.Length));

				Console.WriteLine("Sensitivity: " + SE);
				Console.WriteLine("Specificity: " + SP);
			}
			else
			{
				Thread readerThread = new Thread(new ThreadStart(reader.Start));
				readerThread.Start();
				currentCount = 0;

				Thread readLineThread = new Thread(new ThreadStart(readInput));
				readLineThread.Start();
			}
		}

		private void readInput()
		{
			while (true)
			{
				ConsoleKeyInfo k = Console.ReadKey();
				if (k.Key == ConsoleKey.Spacebar)
				{
					if (!Training)
					{
						Training = true;
					}

					if (Mode == TrainingMode.EYES_CLOSED)
					{
						Mode = TrainingMode.EYES_OPENED;
					}
					else
					{
						Mode = TrainingMode.EYES_CLOSED;
					}
				}
				else if (k.Key == ConsoleKey.F)
				{
					Training = false;
				}
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
					if (Mode != TrainingMode.IDLE && currentCount >= IGNORE_COUNT)
					{
						data.Add(packet.Data[0]);
						modes.Add(mode);
					}


					if (Mode != TrainingMode.IDLE)
					{
						currentCount++;
					}
				}
				else if (svm != null)
				{
					data.Add(packet.Data[0]);
					double[][] features = new double[data.Count / 2][];
					int i = 0;
					while (data.Count > 2)
					{
						double[] feature = new double[2];
						feature[0] = data[0];
						feature[1] = data[1];
						features[i++] = feature;
						data.RemoveAt(0);
						data.RemoveAt(1);
					}

					bool[] answers = svm.Decide(features);
					foreach (bool val in answers)
					{
						if (val)
						{
							Console.WriteLine("Open");
						}
						else
						{
							Console.WriteLine("Closed");
						}
					}
				}


			}
		}

		private DecisionTree Train(double[][] features, int[] outputs)
		{

			DecisionTree tree = new DecisionTree(
				inputs: new List<DecisionVariable>
					{
						DecisionVariable.Continuous("X"),
						DecisionVariable.Continuous("Y")
					},
				classes: 2);

			C45Learning teacher = new C45Learning(tree);
			teacher.Learn(features, outputs);

			return tree;

			//var learner = new NaiveBayesLearning<NormalDistribution>();

			//var nb = learner.Learn(features, outputs);
			//return nb;

			//var teacher = new LinearCoordinateDescent();

			//// Teach the vector machine
			//SupportVectorMachine svm = teacher.Learn(features, outputs);

			//return svm;
		}
		private void Train()
		{
			LinearCoordinateDescent teacher = new LinearCoordinateDescent();

			double[][] features = new double[data.Count / 2][];
			int[] outputs = new int[data.Count / 2];

			for (int i = 0; i < data.Count; i += 2)
			{
				if (modes[i] == modes[i + 1])
				{
					double[] feature = new double[2];
					feature[0] = data[i];
					feature[1] = data[i + 1];
					features[i / 2] = feature;
					outputs[i / 2] = (int)Mode;
				}
				else
				{
					i--;
				}
			}

			// Teach the vector machine

			svm = teacher.Learn(features, outputs);
			data.Clear();
		}

		private void SaveToCSV()
		{
			StringBuilder csv = new StringBuilder();

			for (int i = 0; i < data.Count; i += 2)
			{
				if (i + 1 < modes.Count && modes[i] == modes[i + 1])
				{
					string newLine = string.Format("{0},{1},{2}", data[i], data[i + 1], (int)modes[i]);
					csv.AppendLine(newLine);
				}
			}

			string filePath = Directory.GetCurrentDirectory();
			filePath += "/" + string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now) + ".csv";
			Console.WriteLine("Saving to: " + filePath);
			File.WriteAllText(filePath, csv.ToString());
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
			return lines;
		}

	}
}
