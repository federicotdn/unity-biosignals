using System;
using System.Threading;

namespace pfcore
{
	enum RunMode
	{
		EEG, EMG, EKG, EMGWrite, EEGWrite, EEGTrain, EEGSession, EMGAnalysis
	}

	class MainClass
	{
		public static void Main(string[] args)
		{
#if !UNITY_EDITOR
			Console.WriteLine("Hello from pfcore.Main");
			if (args.Length < 1)
			{
				Console.WriteLine("Invalid number of arguments.");
				return;
			}

			RunMode runMode;
			Enum.TryParse(args[0], true, out runMode);

			switch (runMode)
			{
				case RunMode.EEG:
					Console.WriteLine("Running on EEG mode!\n");
					if (args.Length >= 2)
					{
						RunEEG(args[1]);
					}
					else
					{
						RunEEG();
					}
					break;
				case RunMode.EMG:
					Console.WriteLine("Running on EMG mode!\n");
					RunEMG();
					break;
				case RunMode.EKG:
					Console.WriteLine("Running on EKG mode!\n");
					RunEKG();
					break;
				case RunMode.EEGWrite:
					Console.WriteLine("Running on EEGWrite mode!\n");
					RunEEGWrite();
					break;
				case RunMode.EEGTrain:
					Console.WriteLine("Running on EEGTrain mode!\n");
					RunEEGTrain(args[1], args[2]);
					break;
				case RunMode.EEGSession:
					Console.WriteLine("Running on EEG Session mode!\n");
					if (args.Length >= 3)
					{
						RunEEGSession(args[1], args[2]);
					}
					else
					{
						RunEEGSession();
					}
					break;
				case RunMode.EMGAnalysis:
					Console.WriteLine("Running EMG Analysis on file.");
					RunEMGAnalysis(args[1]);
					break;
				default:
					throw new Exception("No run mode specified.");

			}
#endif
		}

		private static void RunEEG(string filepath = null)
		{
			EEGReader reader;
			if (filepath != null)
			{
				reader = new EEGFileReader(filepath, true);
			}
			else
			{
				reader = new EEGOSCReader(5005);
			}

			EEGProcessor processor = new EEGProcessor(reader);

			processor.Start();
			while (true)
			{
				processor.Update();
			}
		}

		private static void RunEEGWrite()
		{
			EEGReader reader = new EEGOSCReader(5005);
			EEGWriter writer = new EEGWriter(reader);
			writer.Start();

		}

		private static void RunEEGTrain(string trainingSet, string predictionSet)
		{
			EEGAnalysis analysis = new EEGAnalysis(trainingSet, predictionSet);
			analysis.Start();
		}

		private static void RunEKG()
		{
			EKGReader reader = new EKGReader("/dev/tty.SLAB_USBtoUART", 500);
			EKGProcessor processor = new EKGProcessor(reader, 30);
			processor.Start();

			int previousBPM = 0;

			while (true)
			{
				processor.Update();
				int bpm = processor.GetBPM();
				if (previousBPM != bpm)
				{
					previousBPM = bpm;
					Console.WriteLine(bpm);
				}

			}
		}

		private static void RunEMGAnalysis(string filename)
		{
			EMGAnalysis analysis = new EMGAnalysis(filename);
			analysis.PrintResults();
			Console.WriteLine("Press any key to exit.");
			Console.Read();
		}


		private static void RunEMG()
		{
			EMGSerialReader reader = new EMGSerialReader("COM4", 1000);
			EMGProcessor processor = new EMGProcessor(reader);
			processor.Start();

			long ticks = DateTime.Now.Ticks;

			while (true)
			{
				//long dtTicks = DateTime.Now.Ticks - ticks;

				//float dt = (float)dtTicks / 10000000;

				processor.Update();

				//ticks = DateTime.Now.Ticks;

				Thread.Sleep(16);
			}
		}

		private static void RunEEGSession(string trainigPath = null, string predictionPath = null)
		{
			EEGReader reader = new EEGOSCReader(5005);

			EEGSessionRecorder sessionRecorder;
			if (trainigPath != null && predictionPath != null)
			{
				EEGProcessor processor = new EEGProcessor(reader);
				sessionRecorder = new EEGSessionRecorder(processor, trainigPath, predictionPath);
			}
			else
			{
				EEGProcessor processor = new EEGProcessor(reader);
				sessionRecorder = new EEGSessionRecorder(processor);
			}

			sessionRecorder.Start();
		}
	}
}


