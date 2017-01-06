using System;

using Accord.MachineLearning;
using System.Threading;

namespace pfcore
{
	enum RunMode
	{
		EEG, EMG, EKG
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

			EEGProcessor processor;
			if (trainigPath != null && predictionPath != null)
			{
				processor = new EEGProcessor(reader, trainigPath, predictionPath);
			}
			else
			{
				processor = new EEGProcessor(reader, false);
			}

			processor.Start();
			while (true)
			{
				processor.Update();
			}
		}

		private static void RunEKG()
		{

		}

		private static void RunEMG()
		{
            EMGReader reader = new EMGReader("COM4", 1000);
            EMGProcessor processor = new EMGProcessor(reader);
            processor.Start();

            long ticks = DateTime.Now.Ticks;

            while (true) {
                long dtTicks = DateTime.Now.Ticks - ticks;

                float dt = (float)dtTicks / 10000000;

                processor.Update();

                ticks = DateTime.Now.Ticks;

                Thread.Sleep(16);
            }
        }
    }
}
