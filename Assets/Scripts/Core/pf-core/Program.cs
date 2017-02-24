using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using OSC;
using System.Text;

namespace pfcore
{
	enum RunMode
	{
		EEG, EMG, EKG, EMGWrite, EEGWrite, EMGCSV, EEGConvert, EMGCrossVal, EEGCrossVal, EEGJoin, EEGCSV, SPO2Writer
	}

	class MainClass
	{
		internal static void Main(string[] args)
		{
#if (!UNITY_EDITOR && !UNITY_STANDALONE)
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
				case RunMode.EMGCSV:
					Console.WriteLine("Running EMG Analysis on file.");
					RunEMGCSV(args[1]);
					break;
				case RunMode.EEGConvert:
					Console.WriteLine("Running EEG Convert");
					RunEEGConvert(args[1]);
					break;
				case RunMode.EMGCrossVal:
					Console.WriteLine("Running EMG Cross Validation");
					RunEMGCrossValidation(args[1]);
					break;
				case RunMode.EEGCrossVal:
					Console.WriteLine("Running EEG Cross Validation");
					RunEEGCrossValidation(args[1]);
					break;
				case RunMode.EEGJoin:
					Console.WriteLine("Running EEG Join\n");
					RunEEGJoin(args[1], args[2], args[3]);
					break;
				case RunMode.EEGCSV:
					Console.WriteLine("Running EEG CSV");
					RunEEGCSV(args[1]);
					break;
                case RunMode.SPO2Writer:
                    Console.WriteLine("Running SPO2Writer");
                    RunSPO2Writer();
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
				reader = new EEGFileReader(filepath);
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

        private static void RunSPO2Writer() {
            SPO2Writer writer = new SPO2Writer("COM5");
            writer.Start();
        }

		private static void RunEEGWrite()
		{
			EEGReader reader = new EEGOSCReader(5005);
			EEGWriter writer = new EEGWriter(reader);
			writer.Start();

		}

		private static void RunEKG()
		{
			SPO2Reader reader = new SPO2Reader("/dev/tty.SLAB_USBtoUART", 500);
			SPO2Processor processor = new SPO2Processor(reader);
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

		private static void RunEMGCSV(string filename)
		{
			EMGCSV analysis = new EMGCSV(filename);
			analysis.CreateCSV();
			Console.WriteLine("Press any key to exit.");
			Console.ReadKey();
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

		private static void RunEEGConvert(string filepath)
		{
			EEGConverter converter = new EEGConverter(filepath);
			converter.Run();

		}

		private static void RunEEGCrossValidation(string directoryPath)
		{
			List<string> filepaths = getFiles(directoryPath, "*.eeg");
			List<List<TrainingValue>> dataSets = new List<List<TrainingValue>>();
			foreach (string filepath in filepaths)
			{
				List<TrainingValue> data = EEGAnalysis.getTrainingValues(filepath);
				dataSets.Add(data);
			}

			List<int> ks = new List<int> {
				5,
				10,
				15
			};

			CrossValidation.CrossValidate(dataSets, ks, EEGProcessor.FEATURE_COUNT, filepaths);
		}

		private static void RunEMGCrossValidation(string directoryPath) {
			List<string> filepaths = getFiles(directoryPath, "*.emg");
			List<List<TrainingValue>> dataSets = new List<List<TrainingValue>>();
			foreach (string filepath in filepaths) {
				List<EMGPacket> packets = EMGCSV.ReadPackets(filepath);
				List<TrainingValue> data = EMGCSV.GetTrainingValues(packets, false);
				dataSets.Add(data);
			}

			List<int> ks = new List<int> {
				5,
				10,
				15
			};

			CrossValidation.CrossValidate(dataSets, ks, EMGProcessor.FEATURE_COUNT, filepaths);

            Console.ReadKey();
		}

		private static void RunEEGJoin(string file1, string file2, string newName) {
			
			EEGReader reader;
			reader = new EEGFileReader(file1);
			reader.Start();
			List<OSCPacket> packets = new List<OSCPacket>();
			OSCPacket packet;
			while (reader.TryDequeue(out packet)) {
				packets.Add(packet);
			}

			Console.WriteLine("Read " + packets.Count + " packages from " + file1);
			int prevCount = packets.Count;

			reader = new EEGFileReader(file2);
			reader.Start();
			while (reader.TryDequeue(out packet)) {
				packets.Add(packet);
			}

			Console.WriteLine("Read " + (packets.Count - prevCount) + " packages from " + file2);

			FileStream stream = File.OpenWrite(newName);
			foreach (OSCPacket p in packets)
			{
				byte[] bytes = p.Pack();
				stream.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
				stream.Write(bytes, 0, bytes.Length);
			}
			stream.Close();

			Console.WriteLine("Saved new dataset to: " + newName);
		}

		private static List<string> getFiles(string path, string extension) {
			FileAttributes attr = File.GetAttributes(path);
			if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
				string[] paths = Directory.GetFiles(path, extension, SearchOption.TopDirectoryOnly);
				List<string> filepaths = new List<string>(paths);


				return filepaths;
			}

			return new List<string> {
				path
			};
		}

		private static void RunEEGCSV(string filepath)
		{
			EEGReader reader = new EEGFileReader(filepath);
			EEGProcessor processor = new EEGProcessor(reader, true);

			processor.Start();

			while (!processor.Finished)
			{
				processor.Update();
			}

			List<TrainingValue> results = processor.TrainingValues;


			StringBuilder csv = new StringBuilder();

			foreach (TrainingValue t in results)
			{
				csv.AppendLine(String.Format("{0},{1},{2}", (t.Features[0] + t.Features[1]) / 2, (t.Features[2] + t.Features[3]) / 2, t.State));

			}

			string filename = filepath.Split('.')[0] + ".csv";
			//string filename2 = filename + "-2.csv";

			//filename += "-1.csv";


			File.WriteAllText(filename, String.Empty);
			File.WriteAllText(filename, csv.ToString());
			//File.WriteAllText(filename2, String.Empty);
			//File.WriteAllText(filename2, csv2.ToString());
		}
	}
}


