using System.IO;
using System.Threading;

namespace pfcore {
    class EMGWriter {
        private EMGReader reader;
        private FileStream outStream;
        private Thread readerThread;

        public EMGWriter(EMGReader reader, FileStream outStream) {
            this.reader = reader;
            this.outStream = outStream;
        }

        public void StartWrite() {
            readerThread = new Thread(new ThreadStart(reader.Start));
            readerThread.Start();
        }

        public void Update() {
            ConcurrentQueue<EMGPacket> queue = reader.PacketQueue;

            EMGPacket packet;
            byte[] buffer = new byte[EMGPacket.PACKET_SIZE];

            while (queue.TryDequeue(out packet)) {
                packet.Pack(buffer);
                outStream.Write(buffer, 0, buffer.Length);
            }
        }

        public void StopAndJoin() {
            reader.Stop();
            readerThread.Join();
        }
    }
}
