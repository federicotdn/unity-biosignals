using System;

using Accord.MachineLearning;
using System.Threading;
using System.IO;

namespace pfcore
{
	enum RunMode
	{
		EEG, EMG, EKG, EMGWrite
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
					if (args.Length >= 3)
					{
						RunEEG(args[1], args[2]);
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
				case RunMode.EMGWrite:
					Console.WriteLine("Running in EMG Write mode.");
					RunEMGWrite();
					break;
				case RunMode.EKG:
					Console.WriteLine("Running on EKG mode!\n");
					RunEKG();
					break;
				default:
					throw new Exception("No run mode specified.");

			}
#endif
		}

		private static void RunEEG(string trainigPath = null, string predictionPath = null)
		{
			EEGReader reader = new EEGReader(5005);

			EEGProcessor processor = new EEGProcessor(reader, false);
			EEGSessionRecorder sessionRecorder;
			if (trainigPath != null && predictionPath != null)
			{
				sessionRecorder = new EEGSessionRecorder(processor, trainigPath, predictionPath);
			}
			else
			{
				sessionRecorder = new EEGSessionRecorder(processor);
			}

			sessionRecorder.Start();
		}

		private static void RunEKG()
		{

		}

		private static void RunEMGWrite()
		{
			string filename = DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".emg";

			FileStream outStream = File.OpenWrite(filename);

			EMGReader reader = new EMGReader("COM4", 1000);

			Thread readerThread = new Thread(new ThreadStart(reader.Start));
			readerThread.Start();

			EMGWriter writer = new EMGWriter(reader, outStream);

			Console.WriteLine("Starting EMG writer, file: " + outStream.Name);

			DateTime start = DateTime.Now;
			DateTime last = DateTime.Now;
			while (DateTime.Now.Subtract(start).TotalSeconds < 60)
			{
				writer.Update();

				if (DateTime.Now.Subtract(last).TotalSeconds > 1)
				{
					Console.Write((int)DateTime.Now.Subtract(start).TotalSeconds);
					Console.Write("... ");
					last = DateTime.Now;
				}

				Thread.Sleep(16);
			}

			outStream.Close();

			reader.Stop();
			readerThread.Join();

			Console.WriteLine();
			Console.WriteLine("EMG write completed. Press Enter to exit.");

			Console.ReadLine();
		}

		private static void RunEMG()
		{
			EMGReader reader = new EMGReader("COM4", 1000);
			EMGProcessor processor = new EMGProcessor(reader);
			processor.Start();

			long ticks = DateTime.Now.Ticks;

			while (true)
			{
				long dtTicks = DateTime.Now.Ticks - ticks;

				float dt = (float)dtTicks / 10000000;

				processor.Update();

				ticks = DateTime.Now.Ticks;

				Thread.Sleep(16);
			}
		}
	}
}
