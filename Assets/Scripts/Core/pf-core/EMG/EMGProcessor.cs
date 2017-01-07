using System;
using System.Collections.Generic;
using System.Threading;
using System.Numerics;

using Accord.Math;

namespace pfcore {
    class EMGProcessor {
        private EMGReader reader;

        public const int FFT_SAMPLE_SIZE = 256;
        public const double SAMPLE_RATE = 256; // Sample rate for Olimex EMG 
        public const double FREQ_STEP = SAMPLE_RATE / FFT_SAMPLE_SIZE;

        private List<double> readings = new List<double>();
        private Complex[] fftResults = new Complex[FFT_SAMPLE_SIZE];
        public Complex[] FFTResults {
            get {
                return fftResults;
            }
        }

        public EMGProcessor(EMGReader reader) {
            this.reader = reader;
        }

        public void Start() {
            Thread readerThread = new Thread(new ThreadStart(reader.Start));
            readerThread.Start();
        }

        public void Update() {
            ConcurrentQueue<EMGPacket> queue = reader.PacketQueue;

            EMGPacket packet;
            while (queue.TryDequeue(out packet)) {
                readings.Add(packet.channels[0] / 1000.0f);
            }

            if (readings.Count >= FFT_SAMPLE_SIZE) {
                fftResults = RunFFT(readings);
                readings.Clear();
            }

            while (queue.TryDequeue(out packet)) {
                /* Discard packets */
            }
        }

        private Complex[] RunFFT(List<double> values) {
            Complex[] data = new Complex[values.Count];
            for (int i = 0; i < values.Count; i++) {
                data[i] = new Complex(values[i], 0);
            }

            FourierTransform.DFT(data, FourierTransform.Direction.Forward);
            return data;
        }
    }
}
