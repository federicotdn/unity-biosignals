using System;
using System.Collections.Generic;
using System.Threading;

namespace pfcore {
    class SPO2Processor
	{
		private SPO2Reader reader;
		private List<long> peaks = new List<long>();
		private Thread readerThread;

        private List<SPO2Packet> processedPackets = new List<SPO2Packet>();
        public List<SPO2Packet> ProcessedPackets {
            get {
                return processedPackets;
            }
        }

		private const int AVG_COUNT = 10;
        private const int MAX_BPM = 160;
        private long lastPeakTime;

		public SPO2Processor(SPO2Reader reader)
		{
			this.reader = reader;
		}

		public void Start()
		{
            lastPeakTime = DateTime.Now.Ticks;
			readerThread = new Thread(new ThreadStart(reader.Start));
			readerThread.Start();
		}

		public void Update()
		{
			ConcurrentQueue<SPO2Packet> queue = reader.PacketQueue;
            processedPackets.Clear();
            SPO2Packet packet;

			while (queue.TryDequeue(out packet))
			{
				if (packet.Peak && TimeSpan.FromTicks(packet.timeStamp - lastPeakTime).TotalSeconds > 60.0f / MAX_BPM)
				{
					peaks.Add(packet.timeStamp);
                    lastPeakTime = packet.timeStamp;
				}

				// Discard old packages
				while (peaks.Count > AVG_COUNT)
				{
					peaks.RemoveAt(0);
				}

                processedPackets.Add(packet);
            }

			///* Discard packets received during processing */
			//queue.Clear();
		}

		public void StopAndJoin()
		{
			reader.Stop();
			readerThread.Join();
		}

		public int GetBPM()
		{
			if (peaks.Count < AVG_COUNT)
			{
				return 0;
			}

			double aux = 0;
			int beatsCount = Math.Min(AVG_COUNT, peaks.Count) - 1;
			for (int i = 0; i < beatsCount; i++)
			{
				long t1 = peaks[peaks.Count - i - 1];
				long t2 = peaks[peaks.Count - (i + 1) - 1];
				aux += (60 / TimeSpan.FromTicks(t1 - t2).TotalSeconds);

			}

			return (int)(aux / beatsCount);
		}
	}
}
