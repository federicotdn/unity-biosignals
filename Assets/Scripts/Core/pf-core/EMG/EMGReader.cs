using System;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace pfcore {
    public class EMGReader {
        // Values for Olimexino328 Arduino sketch
        private const int baudRate = 57600;
        private const Parity parity = Parity.None;
        private const int dataBits = 8;
        private const StopBits stopBits = StopBits.One;

        private SerialPort serialPort = null;

        private bool running = true;

        private bool fileMode = false;
        private FileStream fileStream;

        private ConcurrentQueue<EMGPacket> packetQueue = new ConcurrentQueue<EMGPacket>();
        public ConcurrentQueue<EMGPacket> PacketQueue {
            get {
                return packetQueue;
            }
        }
        private int maxQueueSize;

        public EMGReader(string portName, int maxQueueSize) {
            serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            serialPort.ReadTimeout = SerialPort.InfiniteTimeout;
            this.maxQueueSize = maxQueueSize;
        }

        public EMGReader(FileStream fileStream, int maxQueueSize) {
            this.fileStream = fileStream;
            this.maxQueueSize = maxQueueSize;
            fileMode = true;
        }

        public void Stop() {
            running = false;
        }

        private int GetNextByte() {
            if (fileMode) {
                int value = fileStream.ReadByte();

                if (value == -1) {
                    // Start from the beggining of file, again
                    fileStream.Seek(0, SeekOrigin.Begin);
                    return fileStream.ReadByte();
                } else {
                    return value;
                }
            } else {
                return serialPort.ReadByte();
            }
        }

        public void Start() {
            if (serialPort != null) {
                serialPort.Open();
            }

            byte[] buffer = new byte[EMGPacket.PACKET_SIZE];

            while (running) {
                bool readOk = true;
                EMGPacket packet = new EMGPacket();

                for (int i = 0; i < buffer.Length && readOk; i++) {
                    int val = GetNextByte();
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

                    if (fileMode) {
                        // Emulate serial port delay
                        Thread.Sleep(1);
                    }

                    packet.Unpack(buffer);
                    packetQueue.Enqueue(packet);

                    while (packetQueue.Count > maxQueueSize) {
                        EMGPacket temp;
                        packetQueue.TryDequeue(out temp);
                    }
                }
            }

            if (serialPort != null) {
                serialPort.Close();
            }
        }
    }
}
