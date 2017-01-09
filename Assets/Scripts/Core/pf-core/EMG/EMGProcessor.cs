using System;
using System.Collections.Generic;
using System.Threading;
using System.Numerics;

using Accord.Math;

namespace pfcore {
    class EMGProcessor {
        private EMGReader reader;

        public const int FFT_SAMPLE_SIZE = 256;
        public const double FREQ_STEP = EMGPacket.SAMPLE_RATE / FFT_SAMPLE_SIZE;

        private Thread readerThread;

        private List<EMGPacket> readings = new List<EMGPacket>();
        public List<EMGPacket> Readings {
            get {
                return readings;
            }
        }

        private List<Complex> fftResults = new List<Complex>();
        public List<Complex> FFTResults {
            get {
                return fftResults;
            }
        }

        private Action fftCallback = null;
        public Action FFTCallback {
            set {
                fftCallback = value;
            }
        }

        public EMGProcessor(EMGReader reader) {
            this.reader = reader;
        }

        public void Start() {
            readerThread = new Thread(new ThreadStart(reader.Start));
            readerThread.Start();
        }

        public void StopAndJoin() {
            reader.Stop();
            readerThread.Join();
        }

        public void Update() {
            ConcurrentQueue<EMGPacket> queue = reader.PacketQueue;

            EMGPacket packet;
            while (queue.TryDequeue(out packet)) {
                readings.Add(packet);
            }

            if (readings.Count >= FFT_SAMPLE_SIZE) {
                RunFFT();
                if (fftCallback != null) {
                    fftCallback();
                }
                readings.Clear();
            }

            while (queue.TryDequeue(out packet)) {
                /* Discard packets */
            }
        }

        private void RunFFT() {
            Complex[] data = new Complex[readings.Count];
            for (int i = 0; i < readings.Count; i++) {
                data[i] = new Complex(readings[i].channels[0], 0);
            }

            FourierTransform.DFT(data, FourierTransform.Direction.Forward);

            fftResults.Clear();
            fftResults.Capacity = data.Length;
            fftResults.AddRange(data);
        }
    }
}
