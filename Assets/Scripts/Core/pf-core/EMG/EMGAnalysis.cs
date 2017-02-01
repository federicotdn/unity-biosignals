using Accord.MachineLearning.DecisionTrees;
using Accord.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading;

namespace pfcore {
    class EMGAnalysis {
        private string filename;
        private const float TRAINING_COUNT_PCTG = 0.75f;

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

            float mean = 0;
            int count = 0;

            foreach (EMGPacket packet in packets) {
                if (packet.muscleStateHint == MuscleState.NONE) {
                    Console.WriteLine("ERROR: One or more packets are missing a MuscleState hint.");
                    return;
                } else if (packet.muscleStateHint == MuscleState.RELAXED) {
                    mean += packet.channels[0];
                    count++;
                }
            }

            mean /= count;
            Console.WriteLine("Mean for Relaxed packets: " + mean);

            Console.WriteLine("--------------------------------");

            int trainingCount = (int)(TRAINING_COUNT_PCTG * packets.Count);
            trainingCount -= trainingCount % EMGProcessor.FFT_SAMPLE_SIZE;

            int predictionCount = packets.Count - trainingCount;

            Console.WriteLine("Training packets count: " + trainingCount);
            Console.WriteLine("Prediction packets count: " + predictionCount);

            List<EMGPacket> trainingPackets = packets.GetRange(0, trainingCount);
            List<EMGPacket> predictionPackets = packets.GetRange(packets.Count - predictionCount, predictionCount);

            List<TrainingValue> trainingValues = GetTrainingValues(trainingPackets, mean, true);
            List<TrainingValue> predictionValues = GetTrainingValues(predictionPackets, mean, false);

            Console.WriteLine("Training values count: " + trainingValues.Count);
            Console.WriteLine("Prediction values count: " + predictionValues.Count);

            Console.WriteLine("--------------------------------");

            DecisionTree tree = EMGProcessor.CreateDecisionTree();
            EMGProcessor.TrainTree(trainingValues, tree);

            int[,] confMat = new int[2, 2];

            foreach (TrainingValue predValue in predictionValues) {
                int result = tree.Decide(predValue.features);
                MuscleState muscleState = (MuscleState)result;

                int i = (predValue.muscleState == MuscleState.TENSE) ? 1 : 0;
                int j = (muscleState == MuscleState.TENSE) ? 1 : 0;

                confMat[i, j]++;
            }

            Console.WriteLine("Confusion matrix:");
            Console.WriteLine("   R     T");
            Console.WriteLine("R  {0}    {1}", confMat[0, 0], confMat[0, 1]);
            Console.WriteLine("T  {0}    {1}", confMat[1, 0], confMat[1, 1]);

            double sensitivity = (double)confMat[0, 0] / (confMat[0, 0] + confMat[0, 1]);
            double specificity = (double)confMat[1, 1] / (confMat[1, 0] + confMat[1, 1]);
            double accuracy = confMat[0, 0] + confMat[1, 1];
            accuracy /= predictionValues.Count;

            Console.WriteLine("Sensitivity: " + sensitivity);
            Console.WriteLine("Specificity: " + specificity);
            Console.WriteLine("Accuracy: " + accuracy);
        }

        private List<TrainingValue> GetTrainingValues(List<EMGPacket> packets, float mean, bool enableSkip) {
            List<TrainingValue> values = new List<TrainingValue>(EMGProcessor.FFT_SAMPLE_SIZE);
            
            int skipsRemaining = 0;

            for (int i = 0; i < packets.Count / EMGProcessor.FFT_SAMPLE_SIZE; i++) {
                if (enableSkip && skipsRemaining > 0) {
                    skipsRemaining--;
                    continue;
                }

                Complex[] data = new Complex[EMGProcessor.FFT_SAMPLE_SIZE];
                int start = i * EMGProcessor.FFT_SAMPLE_SIZE;
                int end = start + EMGProcessor.FFT_SAMPLE_SIZE;
                MuscleState startMuscleState = packets[start].muscleStateHint;

                for (int j = start; j < end; j++) {
                    EMGPacket packet = packets[j];
                    if (packet.muscleStateHint != startMuscleState) {
                        skipsRemaining += EMGProcessor.SKIPS_AFTER_TRANSITION;
                        break;
                    }

                    data[j - start] = new Complex(packet.channels[0] - mean, 0);
                }

                if (enableSkip && skipsRemaining > 0) {
                    continue;
                }

                FourierTransform.FFT(data, FourierTransform.Direction.Forward);
                List<Complex> fftResults = new List<Complex>(data);

                TrainingValue trainingValue = new TrainingValue(packets[start].muscleStateHint);
                trainingValue.features = EMGProcessor.GetFFTMagnitudes(fftResults, TrainingValue.FEATURE_COUNT);

                values.Add(trainingValue);
            }

            return values;
        }

        private List<EMGPacket> ReadPackets() {
            FileStream fileStream = File.OpenRead(filename);
            List<EMGPacket> packets = new List<EMGPacket>();

            EMGFileReader reader = new EMGFileReader(fileStream, -1);
            Thread readerThread = new Thread(new ThreadStart(reader.Start));

            reader.DisableSerialDelay();
            readerThread.Start();

            EMGPacket packet;
            readerThread.Join();

            while (reader.TryDequeue(out packet)) {
                packets.Add(packet);
            }

            return packets;
        }
    }
}
