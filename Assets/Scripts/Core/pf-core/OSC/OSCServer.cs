using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace OSC
{
	public delegate void PacketReceivedEventHandler(OSCServer sender, OSCPacket packet);

	public class OSCServer
	{
		public int Port { get; }
		public event PacketReceivedEventHandler PacketReceivedEvent;

		private UdpClient client;
		private Thread udpThread;
		private IPEndPoint endPoint;

		private Object callbackLock = new Object();

		public OSCServer(int port)
		{
			Port = port;
			client = new UdpClient(port);
			AsyncCallback callBack = new AsyncCallback(ReceiveCallback);
			client.BeginReceive(callBack, null);
			endPoint = new IPEndPoint(IPAddress.Any, 0);
		}

		private void ReceiveCallback(IAsyncResult result)
		{
			Monitor.Enter(callbackLock);

			Byte[] bytes = null;

			try
			{
				bytes = client.EndReceive(result, ref endPoint);
			}
			catch (Exception e)
			{
				throw new Exception(String.Format("Can't recieve at port {0}", Port), e);

			}

			if (bytes != null && bytes.Length > 0)
			{
				bytes.SubArray(0, 8);
				OSCPacket packet = OSCPacket.Unpack(bytes);
				PacketReceivedEvent(this, packet);
			}

			AsyncCallback callBack = new AsyncCallback(ReceiveCallback);
			client.BeginReceive(callBack, null);
			Monitor.Exit(callbackLock);
		}

		public void Close() {
			PacketReceivedEvent = null;
			client.Close();
		}
	}
}
