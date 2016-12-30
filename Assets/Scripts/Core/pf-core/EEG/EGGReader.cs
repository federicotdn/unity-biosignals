using System;
using UnityOSC;

namespace pfcore
{
	public class EGGReader
	{
		private OSCServer server;

		public EGGReader (int port)
		{
			server = new OSCServer(port);
			server.PacketReceivedEvent += OnPacketReceived;
		}

		private void OnPacketReceived(OSCServer server, OSCPacket packet)
		{
			foreach (OSCMessage msg in packet.Data) {
				if (msg.Address == "/muse/elements/alpha_absolute") {
					Console.WriteLine (msg.Data[0]);
				}
			}
		}
	}
}

