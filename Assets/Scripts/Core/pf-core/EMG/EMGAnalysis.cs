using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace pfcore {
    class EMGAnalysis {
        private string filename;

        public EMGAnalysis(string filename) {
            this.filename = filename;
        }

        public void PrintResults() {
            Console.WriteLine("--------------------------------");
            Console.WriteLine("Using file: " + filename);
            List<EMGPacket> packets = ReadPackets();

            Console.WriteLine("Read: " + packets.Count + " packets.");
            int toDiscard = packets.Count % EMGProcessor.FFT_SAMPLE_SIZE;
            Console.WriteLine("Will discard: " + toDiscard + " packets.");
            Console.WriteLine("FFT Samples size is: " + EMGProcessor.FFT_SAMPLE_SIZE);

            packets.RemoveRange(packets.Count - toDiscard, toDiscard);

            Console.WriteLine("--------------------------------");
            Console.WriteLine("New packet count: " + packets.Count);
            Console.WriteLine("(" + (packets.Count / EMGPacket.SAMPLE_RATE) + " seconds of data)");
            Console.WriteLine("(" + (packets.Count / EMGProcessor.FFT_SAMPLE_SIZE) + " training values)");

            foreach (EMGPacket packet in packets) {
                if (packet.muscleStateHint == MuscleState.NONE) {
                    Console.WriteLine("ERROR: One or more packets are missing a MuscleState hint.");
                    return;
                }
            }


        }

        private List<EMGPacket> ReadPackets() {
            FileStream fileStream = File.OpenRead(filename);
            List<EMGPacket> packets = new List<EMGPacket>();

            EMGFileReader reader = new EMGFileReader(fileStream, -1);
            Thread readerThread = new Thread(new ThreadStart(reader.Start));

            reader.DisableSerialDelay();
            readerThread.Start();

            EMGPacket packet;

            while (reader.Running) {
                while (reader.TryDequeue(out packet)) {
                    packets.Add(packet);
                }
            }

            while (reader.TryDequeue(out packet)) {
                packets.Add(packet);
            }

            reader.Stop();
            readerThread.Join();

            return packets;
        }
    }
}
