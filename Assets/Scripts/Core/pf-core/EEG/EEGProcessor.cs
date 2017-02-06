using System;
using System.Threading;
using System.Collections.Generic;
using OSC;

using System.Numerics;

using Accord.Math;

namespace pfcore
{
	public enum EyesStatus
	{
		CLOSED = 0, OPEN = 1, NONE = 2
	}

	public struct EEGReading
	{
		public float value;
		public long timeStamp;

		public EEGReading(float value, long timeStamp)
		{
			this.value = value;
			this.timeStamp = timeStamp;
		}
	}

	public struct EEGTrainingValue
	{
		public double[] Features;
		public EyesStatus Status;

		public EEGTrainingValue(double[] features, EyesStatus status)
		{
			Features = features;
			Status = status;
		}
	}

	public struct EEGData
	{
		public double[][] features;
		public int[] outputs;

		public EEGData(double[][] features, int[] outputs)
		{
			this.features = features;
			this.outputs = outputs;
		}
	}

	public class EEGProcessor
	{
		public List<float> Alpha { get; private set; }
		public List<float> Beta { get; private set; }
		public List<EyesStatus> AlphaStatus { get; private set; }
		public List<Complex> FFTResults { get; private set; }
		public List<EEGTrainingValue> TrainingValues { get; private set; }
		public List<EEGTrainingValue> TrainingValuesWindow { get; private set; }
		public List<EEGTrainingValue> AlphaTrainingValues { get; private set; }
		public bool Finished { get; private set; }

		public List<Complex> TP9FFT { get; private set; }
		public List<Complex> AF7FFT { get; private set; }
		public List<Complex> AF8FFT { get; private set; }
		public List<Complex> TP10FFT { get; private set; }
		public List<EyesStatus> RawStatus { get; private set; }

		public const int SAMPLING_RATE = 256;
		public const int FFT_SAMPLE_SIZE = 256;
		public const double FREQ_STEP = SAMPLING_RATE / (float)FFT_SAMPLE_SIZE;
		private const int SKIP = 2;

		private List<float> readingsMean = new List<float>();

		private List<float> tp9 = new List<float>();
		private List<float> af7 = new List<float>();
		private List<float> af8 = new List<float>();
		private List<float> tp10 = new List<float>();
		private EyesStatus prevStatus = EyesStatus.OPEN;

		bool alphaSet;
		double alpha1;
		double alpha2;

		public bool Training;

		private int ignore = SKIP;
		private int alphaIgnore = SKIP * 10;

		public Action ProcessorCallback;

		EEGReader reader;

		EyesStatus status;
		public EyesStatus Status
		{
			get
			{
				return status;
			}

			set
			{
				if (value != status)
				{
					status = value;

					af7.Clear();
					af8.Clear();
					tp9.Clear();
					tp10.Clear();

					if (Training && TrainingValues.Count >= SKIP)
					{
						for (int i = 0; i < SKIP; i++)
						{
							TrainingValues.RemoveAt(TrainingValues.Count - 1);
						}

						if (AlphaTrainingValues.Count >= SKIP * 10)
						{
							for (int i = 0; i < SKIP * 10; i++)
							{
								AlphaTrainingValues.RemoveAt(AlphaTrainingValues.Count - 1);
							}
						}
						ignore = SKIP;
						alphaIgnore = SKIP * 10;
					}

					switch (status)
					{
						case EyesStatus.CLOSED:
							Console.WriteLine("Switched to eyes CLOSED");
							break;
						case EyesStatus.OPEN:
							Console.WriteLine("Switched to eyes OPEN");
							break;
					}
				}
			}
		}

		public EEGProcessor(EEGReader reader)
		{
			AlphaStatus = new List<EyesStatus>();
			RawStatus = new List<EyesStatus>();
			Alpha = new List<float>();
			Beta = new List<float>();
			TrainingValues = new List<EEGTrainingValue>();
			AlphaTrainingValues = new List<EEGTrainingValue>();
			TrainingValuesWindow = new List<EEGTrainingValue>();

			FFTResults = new List<Complex>();
			TP9FFT = new List<Complex>();
			AF7FFT = new List<Complex>();
			AF8FFT = new List<Complex>();
			TP10FFT = new List<Complex>();

			this.reader = reader;

		}

		public void Start()
		{
			Thread readerThread = new Thread(new ThreadStart(reader.Start));
			readerThread.Start();
		}

		public void Update()
		{
			OSCPacket packet;

			while (reader.TryDequeue(out packet))
			{
				ProcessPacket(packet);
				if (af7.Count >= FFT_SAMPLE_SIZE)
				{
					RunFFT();
					if (ProcessorCallback != null)
					{
						ProcessorCallback();
					}
					af7.Clear();
					af8.Clear();
					tp9.Clear();
					tp10.Clear();
					TrainingValuesWindow.Clear();
				}
			}

			Finished = reader.Finished;
		}

		private void RunFFT()
		{
			readingsMean.Clear();

			for (int i = 0; i < af7.Count; i++)
			{
				readingsMean.Add(af7[i] + tp9[i] + tp10[i] + af8[i] / 4);
			}

			CalculateFFT(readingsMean, FFTResults);
			CalculateFFT(af7, AF7FFT);
			CalculateFFT(af8, AF8FFT);
			CalculateFFT(tp9, TP9FFT);
			CalculateFFT(tp10, TP10FFT);

			double[] feature = new double[4];
			feature[0] = PSD(TP9FFT, FREQ_STEP);
			feature[1] = PSD(AF7FFT, FREQ_STEP);
			feature[2] = PSD(AF8FFT, FREQ_STEP);
			feature[3] = PSD(TP10FFT, FREQ_STEP);

			if (!Training || (Training && ignore == 0))
			{
				EEGTrainingValue trainingValue = new EEGTrainingValue(feature, Status);
				TrainingValues.Add(trainingValue);
				TrainingValuesWindow.Add(trainingValue);
			}
			else if (Training && ignore != 0)
			{
				ignore--;
			}
		}

		private static double PSD(List<Complex> fft, double step)
		{
			int minIndex = (int)(8 / step);
			int maxIndex = (int)Math.Ceiling(12 / step);

			double ans = 0;
			for (int i = minIndex; i <= maxIndex; i++)
			{
				ans += fft[i].Magnitude * fft[i].Magnitude;
			}

			return ans;
		}

		private void CalculateFFT(List<float> values, List<Complex> fftValues)
		{
			Complex[] data = new Complex[FFT_SAMPLE_SIZE];
			for (int i = 0; i < FFT_SAMPLE_SIZE; i++)
			{
				data[i] = new Complex(values[i], 0);
			}

			FourierTransform.FFT(data, FourierTransform.Direction.Forward);

			fftValues.Clear();
			fftValues.Capacity = data.Length;
			fftValues.AddRange(data);
		}

		private void ProcessPacket(OSCPacket packet)
		{
			foreach (OSCPacket p in packet.Data)
			{
				if (p.IsBundle())
				{
					ProcessPacket(p);
				}
				else
				{
					OSCMessage msg = (OSCMessage)p;

					if ((EyesStatus)msg.Extra != EyesStatus.NONE)
					{
						Status = (EyesStatus)msg.Extra;
					}

					if (msg.Address == "/muse/elements/alpha_absolute")
					{
						Alpha.Add((float)msg.Data[0]);
						if (!Training || (Training && alphaIgnore == 0))
						{
							if (alphaSet)
							{
								alpha2 = (float)msg.Data[0];
								alphaSet = false;

								if (prevStatus == Status)
								{
									double[] features = new double[2];
									features[0] = alpha1;
									features[1] = alpha2;
									AlphaTrainingValues.Add(new EEGTrainingValue(features, Status));
								}
							}
							else
							{
								alpha1 = (float)msg.Data[0];
								alphaSet = true;
							}
						}
						else if (Training && alphaIgnore != 0)
						{
							alphaIgnore--;
						}

						prevStatus = Status;
						AlphaStatus.Add(status);
					}
					else if (msg.Address == "/muse/elements/beta_absolute")
					{
						Beta.Add((float)msg.Data[0]);
					}
					else if (msg.Address == "/muse/eeg")
					{
						tp9.Add((float)msg.Data[0]);
						af7.Add((float)msg.Data[1]);
						af8.Add((float)msg.Data[2]);
						tp10.Add((float)msg.Data[3]);
						RawStatus.Add(status);
					}
				}
			}
		}
	}
}
