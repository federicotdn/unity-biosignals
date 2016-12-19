using System;
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

        public EMGReader(string portName) {
            serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            serialPort.ReadTimeout = SerialPort.InfiniteTimeout;
        }

        public void Start() {
            serialPort.Open();

            byte[] buffer = new byte[EMGPacket.PACKET_SIZE];
            EMGPacket packet = new EMGPacket();

            while (true) {
                bool readOk = true;
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

                    Console.WriteLine("sync0: " + packet.sync0);
                    Console.WriteLine("sync1: " + packet.sync1);
                    Console.WriteLine("count: " + packet.count);
                    Console.WriteLine("version: " + packet.version);
                    Console.WriteLine("switches: " + packet.switches);
                    Console.Write("data: \n[");
                    foreach (byte b in packet.data) {
                        Console.Write(b + ", ");
                    }
                    Console.WriteLine("]");
                    Console.WriteLine("-----");
                } else {
                    Console.WriteLine("skipped");
                    Console.WriteLine("------");
                }
            }
        }
    }
}
