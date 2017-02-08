using System;
using System.Collections.Generic;
using System.Threading;
using System.Numerics;

using Accord.Math;
using Accord.MachineLearning.DecisionTrees;
using Accord.MachineLearning.DecisionTrees.Learning;
using System.IO;

namespace pfcore {
    public class EMGProcessor {
        public enum Mode {
            IDLE,
            DETRENDING,
            TRAINING,
            PREDICTING,
            WRITING
        }

        private EMGReader reader;

        public const int FFT_SAMPLE_SIZE = 128;
        public const double FREQ_STEP = EMGPacket.SAMPLE_RATE / FFT_SAMPLE_SIZE;
        public const int SKIPS_AFTER_TRANSITION = 3;

        private DecisionTree decisionTree;
        private List<TrainingValue> trainingData;
        private MuscleState currentMuscleState = MuscleState.NONE;
        public MuscleState CurrentMuscleState {
            set {
                currentMuscleState = value;
                skipsRemaining = SKIPS_AFTER_TRANSITION;
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
        private int skipsRemaining = 0;

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

            decisionTree = CreateDecisionTree();
            trainingData = new List<TrainingValue>();
        }

        public static DecisionTree CreateDecisionTree() {
            List<DecisionVariable> decisionVariables = new List<DecisionVariable>(TrainingValue.FEATURE_COUNT);
            for (int i = 0; i < TrainingValue.FEATURE_COUNT; i++) {
                decisionVariables.Add(DecisionVariable.Continuous(i.ToString()));
            }

            return new DecisionTree(decisionVariables, TrainingValue.FEATURE_COUNT);
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
                /* Discard packets received during processing */
                reader.ClearQueue();
            }
        }

        private void Train() {
            if (skipsRemaining > 0) {
                skipsRemaining--;
                return;
            }

            TrainingValue value = new TrainingValue(currentMuscleState);
            FillTrainingValue(ref value, fftResults);
            trainingData.Add(value);
        }

        public static void FillTrainingValue(ref TrainingValue value, List<Complex> fftResults) {
            float freqRange = TrainingValue.HIGHER_FREQ - TrainingValue.LOWER_FREQ;
            float freqStep = freqRange / TrainingValue.FEATURE_COUNT;
            float lower = TrainingValue.LOWER_FREQ;
            float higher = lower + freqStep; 

            for (int i = 0; i < TrainingValue.FEATURE_COUNT; i++) {
                value.features[i] = PSD(fftResults, EMGPacket.SAMPLE_RATE, lower, higher);
                lower += freqStep;
                higher += freqStep;
            }
        }

        public static double PSD(List<Complex> fftResults, float sampleFreq, float freqLow, float freqHigh) {
            float freqStep = sampleFreq / fftResults.Count;

            int startIndex = (int)(freqLow / freqStep);
            int stopIndex = (int)(freqHigh / freqStep) + 1;

            double result = 0;

            for (int i = startIndex; i < stopIndex; i++) {
                result += fftResults[i].Magnitude;
            }

            return result;
        }

        private void EndTraining() {
            TrainTree(trainingData, decisionTree);
            trainingData.Clear();
        }

        public static void TrainTree(List<TrainingValue> trainingData, DecisionTree tree) {
            double[][] featuresArray = new double[trainingData.Count][];
            int[] labels = new int[trainingData.Count];

            for (int i = 0; i < featuresArray.Length; i++) {
                featuresArray[i] = trainingData[i].features;
                labels[i] = (int)trainingData[i].muscleState;
            }

            C45Learning teacher = new C45Learning(tree);
            teacher.Learn(featuresArray, labels);
        }

        private void Predict() {
            TrainingValue tmp = new TrainingValue();
            FillTrainingValue(ref tmp, fftResults);

            int result = decisionTree.Decide(tmp.features);
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
