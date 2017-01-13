using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace pfcore
{
	class EKGProcessor
	{
		private EKGReader reader;
		private Queue<long> peaks = new Queue<long>();
		private bool previousPeak;
		private Thread readerThread;

		private int windowSize;

		public EKGProcessor(EKGReader reader, int windowSize)
		{
			this.reader = reader;
			this.windowSize = windowSize;
		}

		public void Start()
		{
			readerThread = new Thread(new ThreadStart(reader.Start));
			readerThread.Start();
		}

		public void Update()
		{
			ConcurrentQueue<EKGPacket> queue = reader.PacketQueue;
			EKGPacket packet;
			while (queue.TryDequeue(out packet))
			{
				if (packet.Peak && !previousPeak)
				{
					peaks.Enqueue(packet.timeStamp);
				}

				previousPeak = packet.Peak;

				// Discard old packages
				while (peaks.Count > 0 && TimeSpan.FromTicks(DateTime.Now.Ticks - peaks.Peek()).TotalSeconds > windowSize)
				{
					peaks.Dequeue();
				}
			}

			// Do stuff

			while (queue.TryDequeue(out packet))
			{
				/* Discard packets */
			}
		}

		public void StopAndJoin()
		{
			reader.Stop();
			readerThread.Join();
		}

		public int GetBPM()
		{
			if (peaks.Count < 10)
			{
				return 0;
			}

			return (int)((60 * peaks.Count) / TimeSpan.FromTicks(DateTime.Now.Ticks - peaks.Peek()).TotalSeconds);
		}

	}
}
