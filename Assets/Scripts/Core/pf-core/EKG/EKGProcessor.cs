using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace pfcore {
    class EKGProcessor {
        private EKGReader reader;
        private readonly long baseTime;

        private List<EKGPacket> readings = new List<EKGPacket>();

        public EKGProcessor(EKGReader reader) {
            this.reader = reader;
            baseTime = DateTime.Now.Ticks;
        }

        public void Start() {
            Thread thread = new Thread(new ThreadStart(reader.Start));
            thread.Start();
        }

        public void Update() {
            ConcurrentQueue<EKGPacket> queue = reader.PacketQueue;
            EKGPacket packet;
            while (queue.TryDequeue(out packet)) {
                readings.Add(packet);
            }

            // Do stuff

            while (queue.TryDequeue(out packet)) {
                /* Discard packets */
            }
        }

    }
}
