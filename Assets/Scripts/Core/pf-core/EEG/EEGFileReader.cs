using System;
using System.Collections.Generic;
using System.IO;
using OSC;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;

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
			BinaryFormatter bin = new BinaryFormatter();
			List<OSCPacket> packets = (List<OSCPacket>)bin.Deserialize(stream);

			long baseTime = ((OSCBundle)packets[0]).TimeStamp;
			foreach (OSCPacket packet in packets)
			{
				if (packet.IsBundle())
				{
					OSCBundle bundle = (OSCBundle)packet;
					if (delay && bundle.TimeStamp - baseTime > 0)
					{
						Thread.Sleep(TimeSpan.FromTicks(bundle.TimeStamp - baseTime).Milliseconds);
					}
					PacketQueue.Enqueue(packet);
					baseTime = bundle.TimeStamp;
				}
				else
				{
					PacketQueue.Enqueue(packet);
				}
			}

			Finished = true;
		}
	}
}
