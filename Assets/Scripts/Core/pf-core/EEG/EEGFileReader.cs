using System;
using System.Collections.Generic;
using System.IO;
using OSC;
using System.Threading;

namespace pfcore
{
	public class EEGFileReader : EEGReader
	{
		private string filePath;
		private bool delay;

		public EEGFileReader(string filePath, bool delay)
		{
			this.filePath = filePath;
			this.delay = delay;
		}

		public override void Start()
		{
			FileStream stream = File.OpenRead(filePath);

			bool finished = false;

			byte[] size = new byte[4];

			List<OSCPacket> packets = new List<OSCPacket>();

			while (!finished)
			{
				int bytesRead = stream.Read(size, 0, 4);
				if (bytesRead == 0)
				{
					finished = true;
				}
				else
				{
					int packetSize = BitConverter.ToInt32(size, 0);
					byte[] bytes = new byte[packetSize];
					stream.Read(bytes, 0, packetSize);
					OSCPacket p = OSCPacket.Unpack(bytes);
					packets.Add(p);
				}
			}

			stream.Close();

			foreach (OSCPacket packet in packets)
			{
				PacketQueue.Enqueue(packet);
			}

			Finished = true;
		}

		public override void Stop()
		{
		}
	}
}
