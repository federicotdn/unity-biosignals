using System;
using System.IO.Ports;

namespace pfcore
{
	class EKGReader
	{
		// Values for CMS50D+ CONTEC Pulse Oximeter
		private const int baudRate = 19200;
		private const Parity parity = Parity.Odd;
		private const int dataBits = 8;
		private const StopBits stopBits = StopBits.One;

		private bool reading;

		private SerialPort serialPort;

		public ConcurrentQueue<EKGPacket> PacketQueue { get; }
		private int maxQueueSize;

		public EKGReader(string portName, int maxQueueSize)
		{
			serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
			serialPort.ReadTimeout = SerialPort.InfiniteTimeout;
			PacketQueue = new ConcurrentQueue<EKGPacket>();
			this.maxQueueSize = maxQueueSize;
		}

		private bool highBitOn(byte b)
		{
			return (b & 0x80) == 0x80;
		}


		public void Start()
		{
			serialPort.Open();

			byte[] buffer = new byte[EKGPacket.PACKET_SIZE];

			reading = true;

			while (reading)
			{
				bool readOk = true;
				EKGPacket packet = new EKGPacket();

				for (int i = 0; i < buffer.Length && readOk; i++)
				{
					int val = serialPort.ReadByte();
					if (val == -1)
					{
						throw new Exception("Serial Port: End of stream.");
					}

					byte readByte = (byte)val;

					if ((i == 0 && !highBitOn(readByte)) ||
						(i != 0 && highBitOn(readByte)))
					{

						readOk = false;
					}

					buffer[i] = readByte;
				}

				packet.Unpack(buffer);

				if (readOk && packet.HeartRate != 0)
				{
					PacketQueue.Enqueue(packet);
					while (PacketQueue.Count > maxQueueSize)
					{
						EKGPacket temp;
						PacketQueue.TryDequeue(out temp);
					}
				}
			}

			if (serialPort != null)
			{
				serialPort.Close();
			}
		}

		public void Stop()
		{
			reading = false;
		}
	}
}
