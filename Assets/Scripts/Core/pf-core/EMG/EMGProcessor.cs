using System;
using System.Collections.Generic;
using System.Threading;
using System.Numerics;

using Accord.Math;
using Accord.MachineLearning.DecisionTrees;
using Accord.MachineLearning.DecisionTrees.Learning;
using System.IO;

namespace pfcore {
    class EMGProcessor {
        private struct TrainingValue {
            public double[] features;
            public MuscleState muscleState;

            public TrainingValue(MuscleState muscleState) {
                this.muscleState = muscleState;
                features = new double[FEATURE_COUNT];
            }
        }

        public enum Mode {
            IDLE,
            DETRENDING,
            TRAINING,
            PREDICTING,
            WRITING
        }

        private EMGReader reader;

        public const int FFT_SAMPLE_SIZE = 256;
        public const double FREQ_STEP = EMGPacket.SAMPLE_RATE / FFT_SAMPLE_SIZE;

        public const int FEATURE_COUNT = 16;

        private DecisionTree decisionTree;
        private List<TrainingValue> trainingData;
        private MuscleState currentMuscleState = MuscleState.NONE;
        public MuscleState CurrentMuscleState {
            set {
                currentMuscleState = value;
            }
            get {
                return currentMuscleState;
            }
        }

        private FileStream outFileStream = null;
        public FileStream OutFileStream {
            set {
                outFileStream = value;
            }
        }

        public int TrainingDataLength {
            get {
                return trainingData.Count;
            }
        }

        private MuscleState predictedMuscleState = MuscleState.NONE;
        public MuscleState PredictedMuscleState {
            get {
                return predictedMuscleState;
            }
        }

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

            List<DecisionVariable> decisionVariables = new List<DecisionVariable>(FEATURE_COUNT);
            for (int i = 0; i < FEATURE_COUNT; i++) {
                decisionVariables.Add(DecisionVariable.Continuous(i.ToString()));
            }

            decisionTree = new DecisionTree(decisionVariables, FEATURE_COUNT);
            trainingData = new List<TrainingValue>();
        }

        public void Start() {
            readerThread = new Thread(new ThreadStart(reader.Start));
            readerThread.Start();
        }

        public void StopAndJoin() {
            reader.Stop();
            readerThread.Join();
        }

        public void ChangeMode(Mode newMode) {
            if (mode == Mode.TRAINING) {
                EndTraining();
            } else if (mode == Mode.PREDICTING) {
                predictedMuscleState = MuscleState.NONE;
            } else if (mode == Mode.WRITING) {
                outFileStream = null;
            }

            mode = newMode;

            if (mode == Mode.DETRENDING) {
                mean = 0.0f;
                sampleCount = 0;
            }
        }

        public void Update() {
            EMGPacket packet;
            while (reader.TryDequeue(out packet)) {
                packet.muscleStateHint = currentMuscleState;
                rawReadings.Add(packet);
            }

            if (rawReadings.Count >= FFT_SAMPLE_SIZE) {
                Idle();

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
                    case Mode.WRITING:
                        Write();
                        break;
                    default:
                        break;
                }

                if (processorCallback != null) {
                    processorCallback();
                }

                rawReadings.Clear();
                readings.Clear();
            }

            if (mode != Mode.WRITING) {
                while (reader.TryDequeue(out packet)) {
                    /* Discard packets */
                }
            }
        }

        private void Train() {
            TrainingValue value = new TrainingValue(currentMuscleState);
            value.features = GetFFTMagnitudes(FEATURE_COUNT);

            trainingData.Add(value);
        }

        private double[] GetFFTMagnitudes(int bins) {
            int binSize = (fftResults.Count / 2) / bins; // Use second half of FFT results
            int startIndex = fftResults.Count / 2;

            double[] results = new double[bins];

            for (int i = 0; i < bins; i++) {
                Complex avg = Complex.Zero;
                for (int j = 0; j < binSize; j++) {
                    int valueIdx = startIndex + (i * binSize) + j;
                    avg += fftResults[valueIdx];
                }
                avg /= binSize;

                results[i] = avg.Magnitude;
            }

            return results;
        }

        private void EndTraining() {
            double[][] featuresArray = new double[trainingData.Count][];
            int[] labels = new int[trainingData.Count];

            for (int i = 0; i < featuresArray.Length; i++) {
                featuresArray[i] = trainingData[i].features;
                labels[i] = (int)trainingData[i].muscleState;
            }

            C45Learning teacher = new C45Learning(decisionTree);
            teacher.Learn(featuresArray, labels);

            trainingData.Clear();
        }

        private void Predict() {
            int result = decisionTree.Decide(GetFFTMagnitudes(FEATURE_COUNT));
            predictedMuscleState = (MuscleState)result;
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

        private void Write() {
            if (outFileStream == null) {
                throw new Exception("WRITE mode: outFileStream is null");
            }

            byte[] buffer = new byte[EMGPacket.PACKET_SIZE_W_HINT];
            foreach (EMGPacket packet in rawReadings) {
                packet.Pack(buffer);
                outFileStream.Write(buffer, 0, buffer.Length);
            }
        }
    }
}
