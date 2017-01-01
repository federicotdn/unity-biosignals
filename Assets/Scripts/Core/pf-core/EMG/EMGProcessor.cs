using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

using Accord.Controls;
using ZedGraph;
using System.Drawing;

namespace pfcore {
    class EMGProcessor {
        private EMGReader reader;

        private const int FFT_SAMPLE_SIZE = 128;

        List<double> vals = new List<double>();
        List<double> times = new List<double>();

        private readonly long baseTime;

        ScatterplotBox bp;

        public EMGProcessor(EMGReader reade`r) {
            this.reader = reader;
            baseTime = DateTime.Now.Ticks;
            bp = ScatterplotBox.Show(vals.ToArray());
        }

        public void Start() {
            Thread readerThread = new Thread(new ThreadStart(reader.Start));
            readerThread.Start();
        }

        public void Update() {
            ConcurrentQueue<EMGPacket> queue = reader.PacketQueue;

            EMGPacket packet;
            while (queue.TryDequeue(out packet)) {
                vals.Add(packet.channels[0] / 1000.0f);
                times.Add((double)(packet.timeStamp - baseTime) / 10000000.0);
            }

            bp.Invoke(new Action<double[], double[]>((double[] xs, double[] ys) => {
                ZedGraphControl zgc = bp.ScatterplotView.Graph;
                zgc.GraphPane.CurveList.Clear();

                zgc.GraphPane.AddCurve("vals", xs, ys, Color.Blue, SymbolType.Circle);

                zgc.AxisChange();
                zgc.Invalidate();

            }), times.ToArray(), vals.ToArray());

            while (queue.TryDequeue(out packet)) {
                /* Discard packets */
            }

            if (vals.Count > 3000) {
                vals.Clear();
                times.Clear();
            }

            Console.WriteLine(vals.Count);
        }
    }
}
