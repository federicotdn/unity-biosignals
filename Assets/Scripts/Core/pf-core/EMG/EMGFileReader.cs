using System.IO;
using System.Threading;

namespace pfcore {
    class EMGFileReader : EMGReader {
        private FileStream fileStream;
        private bool loopFile = false;

        public EMGFileReader(FileStream fileStream, int maxQueueSize) : base(maxQueueSize) {
            this.fileStream = fileStream;
        }

        public void EnableFileLoop() {
            loopFile = true;
        }

        private int GetNextByte() {
            int value = fileStream.ReadByte();
            if (value == -1 && loopFile) {
                // Start from the beggining of file, again
                fileStream.Seek(0, SeekOrigin.Begin);
                return GetNextByte();
            } else {
                return value;
            }
        }

        public override void Start() {
            byte[] buffer = new byte[EMGPacket.PACKET_SIZE_W_HINT];

            while (running) {
                EMGPacket packet = new EMGPacket();
                bool readOk = true;

                for (int i = 0; i < buffer.Length; i++) {
                    int val = GetNextByte();
                    if (val == -1) {
                        running = false;
                        readOk = false;
                    }

                    byte readByte = (byte)val;
                    buffer[i] = readByte;
                }

                if (readOk) {
                    Thread.Sleep(2);

                    packet.Unpack(buffer);
                    packetQueue.Enqueue(packet);
                }

                while (packetQueue.Count > maxQueueSize) {
                    EMGPacket temp;
                    packetQueue.TryDequeue(out temp);
                }
            }

            fileStream.Close();
        }
    }
}
