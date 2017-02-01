using System;
using System.Collections.Generic;
using System.Threading;
using OSC;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace pfcore
{
	public class EEGWriter
	{
		private EEGReader reader;

		private EyesStatus status;
		private List<OSCPacket> packets = new List<OSCPacket>();

		private bool finished = false;
		private bool started = false;

		private Thread readerThread;
		private Thread processingThread;

		public EEGWriter(EEGReader reader)
		{
			this.reader = reader;
		}

		public void Start()
		{
			while (true)
			{
				ConsoleKeyInfo k = Console.ReadKey();
				if (k.Key == ConsoleKey.Spacebar)
				{
					if (!started)
					{
						Console.WriteLine("Started reading!");
						status = EyesStatus.OPEN;
						Console.WriteLine("Switched to eyes open");
						started = true;
						processingThread = new Thread(new ThreadStart(StartReading));
						processingThread.Start();
					}
					else
					{
						if (status == EyesStatus.CLOSED)
						{
							Console.WriteLine("Switched to eyes open");
							status = EyesStatus.OPEN;
						}
						else
						{

							Console.WriteLine("Switched to eyes closed");
							status = EyesStatus.CLOSED;
						}
					}
				}
				else if (k.Key == ConsoleKey.F)
				{
					finished = true;
					processingThread.Join();
					string baseFilePath = Directory.GetCurrentDirectory();
					baseFilePath += "/" + string.Format("/{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now) + ".eeg";

					Console.WriteLine("Saving to raw packets to: " + baseFilePath);

					FileStream stream = File.Create(baseFilePath);
					BinaryFormatter bin = new BinaryFormatter();
					bin.Serialize(stream, packets);

					return;
				}
			}
		}

		private void StartReading()
		{
			readerThread = new Thread(new ThreadStart(reader.Start));
			readerThread.Start();

			while (true)
			{
				if (finished)
				{
					return;
				}

				OSCPacket packet;

				while (reader.TryDequeue(out packet))
				{
					packet.Extra = (byte)status;
					if (packet.IsBundle())
					{
						foreach (OSCPacket p in packet.Data)
						{
							p.Extra = packet.Extra;
						}
					}
					packets.Add(packet);
				}
			}
		}
	}
}

