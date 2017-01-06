using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Numerics;

using Accord.Controls;

using ZedGraph;
using System.Drawing;
using Accord.Math;

namespace pfcore {
    class EMGProcessor {
        private EMGReader reader;

        private const int FFT_SAMPLE_SIZE = 256;
        private const double SAMPLE_RATE = 256; // Sample rate for Olimex EMG 
        private const double FREQ_STEP = SAMPLE_RATE / FFT_SAMPLE_SIZE;

        List<double> readings = new List<double>();
        List<double> times = new List<double>();
 
        private readonly long baseTime;

        ScatterplotBox valuesPlot;
        ScatterplotBox freqsPlot;

        public EMGProcessor(EMGReader reader) {
            this.reader = reader;
            baseTime = DateTime.Now.Ticks;
            valuesPlot = ScatterplotBox.Show(readings.ToArray());

            double[] temp = new double[0];
            freqsPlot = ScatterplotBox.Show(temp);
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

                times.Add((double)(packet.timeStamp - baseTime) / 10000000.0);
            }

            valuesPlot.Invoke(new Action<double[], double[]>((double[] xs, double[] ys) => {
                ZedGraphControl zgc = valuesPlot.ScatterplotView.Graph;
                zgc.GraphPane.CurveList.Clear();

                zgc.GraphPane.AddCurve("vals", xs, ys, Color.Blue, SymbolType.Circle);

                zgc.AxisChange();
                zgc.Invalidate();

            }), times.ToArray(), readings.ToArray());

            while (queue.TryDequeue(out packet)) {
                /* Discard packets */
            }

            if (readings.Count > FFT_SAMPLE_SIZE) {
                Complex[] frequencies = RunFFT(readings);

                freqsPlot.Invoke(new Action<Complex[]>((Complex[] freqs) => {
                    ZedGraphControl zgc = freqsPlot.ScatterplotView.Graph;
                    zgc.GraphPane.CurveList.Clear();

                    double[] freqAmplitudes = new double[freqs.Length];
                    for (int i = 0; i < freqs.Length; i++) {
                        freqAmplitudes[i] = 20 * Math.Log10(freqs[i].Real) * 100;
                    }

                    double[] ranges = new double[freqs.Length];
                    for (int i = 0; i < freqs.Length; i++) {
                        ranges[i] = i * FREQ_STEP;
                    }

                    zgc.GraphPane.AddCurve("FFT", ranges, freqAmplitudes, Color.Blue, SymbolType.Circle);

                    zgc.AxisChange();
                    zgc.Invalidate();

                }), frequencies);

                readings.Clear();
                times.Clear();
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
