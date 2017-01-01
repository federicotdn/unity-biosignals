using System;
using System.Collections.Concurrent;
using System.IO.Ports;

namespace pfcore {
    public class EMGReader {
        // Values for Olimexino328 Arduino sketch
        private const int baudRate = 57600;
        private const Parity parity = Parity.None;
        private const int dataBits = 8;
        private const StopBits stopBits = StopBits.One;
        private const int readTimeout = 500;

        private SerialPort serialPort;

        public ConcurrentQueue<EMGPacket> PacketQueue { get; }
        private int maxQueueSize;

        public EMGReader(string portName, int maxQueueSize) {
            serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            serialPort.ReadTimeout = SerialPort.InfiniteTimeout;
            PacketQueue = new ConcurrentQueue<EMGPacket>();
            this.maxQueueSize = maxQueueSize;
        }

        public void Start() {
            serialPort.Open();

            byte[] buffer = new byte[EMGPacket.PACKET_SIZE];

            while (true) {
                bool readOk = true;
                EMGPacket packet = new EMGPacket();

                for (int i = 0; i < buffer.Length && readOk; i++) {
                    int val = serialPort.ReadByte();
                    if (val == -1) {
                        throw new Exception("Serial Port: End of stream.");
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

                    PacketQueue.Enqueue(packet);

                    while (PacketQueue.Count > maxQueueSize) {
                        EMGPacket temp;
                        PacketQueue.TryDequeue(out temp);
                    }
                }
            }
        }
    }
}
