using OSC;
using System;

namespace pfcore
{
	public abstract class EEGReader
	{
		protected ConcurrentQueue<OSCPacket> PacketQueue = new ConcurrentQueue<OSCPacket>();

		public abstract void Start();

		public bool Finished { get; protected set; }

		public bool TryDequeue(out OSCPacket packet)
		{
			return PacketQueue.TryDequeue(out packet);
		}

		public abstract void Stop();
	}
}


