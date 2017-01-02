using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Collections.Generic;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;

namespace pfcore
{
	public enum TrainingMode
	{
		EYES_OPENED, EYES_CLOSED, IDLE
	}

	public class EEGProcessor
	{
		private EEGReader reader;
		private List<float> data = new List<float>();
		private List<TrainingMode> modes = new List<TrainingMode>();
		private int currentCount;
		private SupportVectorMachine svm;

		private const int IGNORE_COUNT = 10;

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
					Train();
					Console.WriteLine("Finished Training");
				}
			}
		}

		public EEGProcessor(EEGReader reader)
		{
			this.reader = reader;
			Mode = TrainingMode.IDLE;
		}

		public void Start()
		{
			Thread readerThread = new Thread(new ThreadStart(reader.Start));
			readerThread.Start();
			currentCount = 0;

			Thread readLineThread = new Thread(new ThreadStart(readInput));
			readLineThread.Start();
		}

		private void readInput()
		{
			while (true)
			{
				String s = Console.ReadLine();
				if (s == "s")
				{
					Training = true;
				}
				else if (s == "c")
				{
					Mode = TrainingMode.EYES_CLOSED;
				}
				else if (s == "o")
				{
					Mode = TrainingMode.EYES_OPENED;
				}
				else if (s == "f")
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
					if (modes[i] == TrainingMode.EYES_CLOSED)
					{
						outputs[i / 2] = 0;
					}
					else
					{
						outputs[i / 2] = 1;
					}
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
	}
}
