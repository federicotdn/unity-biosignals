using System;

using Accord.MachineLearning;

namespace pfcore
{
	enum RUN_MODE
	{
		EEG = 1, EMG = 2, EKG = 3
	}

	class MainClass
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("Hello from pfcore.Main");
			if (args.Length != 1)
			{
				Console.WriteLine("Invalid number of arguments.");
				return;
			}

			int mode = Int32.Parse(args[0]);

			switch (mode)
			{
				case (int)RUN_MODE.EEG:
					Console.WriteLine("Running on EEG mode!\n");
					RunEEG();
					break;
				case (int)RUN_MODE.EMG:
					Console.WriteLine("Running on EMG mode!\n");
					RunEMG();
					break;
				case (int)RUN_MODE.EKG:
					Console.WriteLine("Running on EKG mode!\n");
					RunEKG();
					break;

			}
		}

		public static string TestAccordClass()
		{
			MinimumMeanDistanceClassifier t = new MinimumMeanDistanceClassifier();
			return t.ToString();
		}

		private static void RunEEG()
		{
			EEGReader reader = new EEGReader(5005);
			EEGProcessor processor = new EEGProcessor(reader);
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

		}
	}
}
