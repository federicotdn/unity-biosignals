using System;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace pfcore {
    public class EMGSerialReader : EMGReader {
        // Values for Olimexino328 Arduino sketch
        private const int baudRate = 57600;
        private const Parity parity = Parity.None;
        private const int dataBits = 8;
        private const StopBits stopBits = StopBits.One;

        private SerialPort serialPort = null;

        public EMGSerialReader(string portName, int maxQueueSize) : base(maxQueueSize) {
            serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            serialPort.ReadTimeout = SerialPort.InfiniteTimeout;
        }

        public override void Start() {
            serialPort.Open();
            byte[] buffer = new byte[EMGPacket.PACKET_SIZE];

            while (running) {
                bool readOk = true;
                EMGPacket packet = new EMGPacket();

                for (int i = 0; i < buffer.Length && readOk; i++) {
                    int val = serialPort.ReadByte();
                    if (val == -1) {
                        throw new Exception("End of stream.");
                    }

                    byte readByte = (byte)val;

                    if ((i == 0 && readByte != EMGPacket.SYNC0_BYTE) ||
                        (i == 1 && readByte != EMGPacket.SYNC1_BYTE) ||
                        (i == 2 && readByte != EMGPacket.VERSION_BYTE)) {

                        readOk = false;
                    }

                    buffer[i] = readByte;
                }

                if (readOk) {
                    packet.Unpack(buffer);
                    packetQueue.Enqueue(packet);

                    while (packetQueue.Count > maxQueueSize) {
                        EMGPacket temp;
                        packetQueue.TryDequeue(out temp);
                    }
                }
            }

            serialPort.Close();
        }
    }
}
