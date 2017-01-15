using System;
using System.Collections.Generic;
using System.Threading;
using System.Numerics;

using Accord.Math;

namespace pfcore {
    class EMGProcessor {

        public enum Mode {
            IDLE,
            DETRENDING,
            TRAINING,
            PREDICTING
        }

        private EMGReader reader;

        public const int FFT_SAMPLE_SIZE = 256;
        public const double FREQ_STEP = EMGPacket.SAMPLE_RATE / FFT_SAMPLE_SIZE;

        private Thread readerThread;
        private Mode mode = Mode.IDLE;
        public Mode CurrentMode {
            get {
                return mode;
            }
        }

        private float mean = 0.0f;
        public float Mean {
            get {
                return mean;
            }
        }
        private int sampleCount = 0;

        private List<EMGPacket> rawReadings = new List<EMGPacket>();
        private List<EMGReading> readings = new List<EMGReading>();
        public List<EMGReading> Readings {
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

        private Action processorCallback = null;
        public Action ProcessorCallback {
            set {
                processorCallback = value;
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

        public void ChangeMode(Mode mode) {
            this.mode = mode;

            if (mode == Mode.DETRENDING) {
                mean = 0.0f;
                sampleCount = 0;
            }
        }

        public void Update() {
            ConcurrentQueue<EMGPacket> queue = reader.PacketQueue;

            EMGPacket packet;
            while (queue.TryDequeue(out packet)) {
                rawReadings.Add(packet);
            }

            if (rawReadings.Count >= FFT_SAMPLE_SIZE) {
                switch (mode) {
                    case Mode.TRAINING:
                        Train();
                        break;
                    case Mode.PREDICTING:
                        Predict();
                        break;
                    case Mode.DETRENDING:
                        Detrend();
                        break;
                    case Mode.IDLE:
                    default:
                        Idle();
                        break;
                }

                if (processorCallback != null) {
                    processorCallback();
                }

                rawReadings.Clear();
                readings.Clear();
            }

            while (queue.TryDequeue(out packet)) {
                /* Discard packets */
            }
        }

        private void Train() {
            
        }

        private void Predict() {

        }

        private void Detrend() {
            foreach (EMGPacket packet in rawReadings) {
                mean = ((mean * sampleCount) + packet.channels[0]) / (sampleCount + 1);
                sampleCount++;
            }
        }

        private void Idle() {
            readings.Clear();
            readings.Capacity = rawReadings.Count;
            foreach (EMGPacket packet in rawReadings) {
                readings.Add(new EMGReading(packet.channels[0] - mean, packet.timeStamp));
            }

            Complex[] data = new Complex[FFT_SAMPLE_SIZE];
            for (int i = 0; i < FFT_SAMPLE_SIZE; i++) {
                data[i] = new Complex(rawReadings[i].channels[0] - mean, 0);
            }

            FourierTransform.FFT(data, FourierTransform.Direction.Forward);

            fftResults.Clear();
            fftResults.Capacity = data.Length;
            fftResults.AddRange(data);
        }
    }
}
