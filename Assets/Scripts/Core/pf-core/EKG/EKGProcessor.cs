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
		private readonly long baseTime;

		private Queue<long> peaks = new Queue<long>();

		private bool previousPeak;


		private const long windowSize = 10;

		public EKGProcessor(EKGReader reader)
		{
			this.reader = reader;
			baseTime = DateTime.Now.Ticks;
		}

		public void Start()
		{
			Thread thread = new Thread(new ThreadStart(reader.Start));
			thread.Start();
		}

		public void Update()
		{
			ConcurrentQueue<EKGPacket> queue = reader.PacketQueue;
			EKGPacket packet;
			while (queue.TryDequeue(out packet))
			{
				if (!previousPeak)
				{
					previousPeak = true;
					peaks.Enqueue(packet.timeStamp);
				}
				else
				{
					previousPeak = false;
				}

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

		public int getBPM()
		{
			if (peaks.Count == 0)
			{
				return 0;
			}

			return (int)((60 * peaks.Count) / TimeSpan.FromTicks(DateTime.Now.Ticks - peaks.Peek()).TotalSeconds);
		}

	}
}
