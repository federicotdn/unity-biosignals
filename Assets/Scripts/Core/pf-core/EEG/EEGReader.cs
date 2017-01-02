using System;
using UnityOSC;
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
			foreach (OSCMessage msg in packet.Data)
			{
				if (msg.Address == "/muse/elements/alpha_absolute")
				{
					float[] data = new float[1];
					data[0] = (float)msg.Data[0];
					PacketQueue.Enqueue(new EEGPacket(DataType.ALPHA, data));
				}
			}
		}
	}
}

