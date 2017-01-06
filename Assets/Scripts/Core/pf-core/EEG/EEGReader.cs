using System;
using OSC;
using System.Collections.Concurrent;

namespace pfcore
{
	public class EEGReader
	{
		private OSCServer server;
		private int port;
		public ConcurrentQueue<EEGPacket> PacketQueue { get; }

		public EEGReader(int port)
		{
			PacketQueue = new ConcurrentQueue<EEGPacket>();
			this.port = port;
		}

		public void Start()
		{
			server = new OSCServer(port);
			server.PacketReceivedEvent += OnPacketReceived;
		}

		private void OnPacketReceived(OSCServer s, OSCPacket packet)
		{
			ProcessPacket(packet);
		}

		private void ProcessPacket(OSCPacket packet)
		{
			foreach (OSCPacket p in packet.Data)
			{
				if (p.IsBundle())
				{
					ProcessPacket(p);
				}
				else
				{
					OSCMessage msg = (OSCMessage)p;
					if (msg.Address == "/muse/elements/alpha_absolute")
					{
						float[] data = new float[1];
						data[0] = (float)msg.Data[0];
						PacketQueue.Enqueue(new EEGPacket(DataType.ALPHA, data));
					}
					//if (msg.Address == "/Marker/1")
					//{
					//	Console.WriteLine("Marker");
					//}
				}
			}
		}
	}
}

