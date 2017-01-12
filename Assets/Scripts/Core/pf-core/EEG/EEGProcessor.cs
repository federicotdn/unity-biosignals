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
using System.Text;

namespace pfcore
{
	public enum TrainingMode
	{
		EYES_OPENED = 1, EYES_CLOSED = 0, IDLE = 2
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
		private EEGReader reader;
		public List<float> Data { get; private set; }
		public List<TrainingMode> Modes { get; private set; }
		private int currentCount;
		private SupportVectorMachine svm;

		private DecisionTree tree;

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
				while (i < IGNORE_COUNT && Data.Count > 0 && i < Data.Count)
				{
					Data.RemoveAt(i);
					Modes.RemoveAt(i);
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
					Console.WriteLine("Finished Training");
				}
			}
		}

		public EEGProcessor(EEGReader reader, bool online)
		{
			Modes = new List<TrainingMode>();
			Data = new List<float>();

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
				currentCount = 0;
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
						Data.Add(packet.Data[0]);
						Modes.Add(mode);
					}


					if (Mode != TrainingMode.IDLE)
					{
						currentCount++;
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



	}
}
