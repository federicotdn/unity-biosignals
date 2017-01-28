using System;

namespace pfcore
{

	public enum DataType
	{
		ALPHA,
		BETA,
		RAW
	}

	public class EEGPacket
	{
		public DataType Type { get; }
		public float[] Data { get; }
		public long timestamp;


		public EEGPacket(DataType type, float[] data, long timestamp)
		{
			this.Type = type;
			this.Data = data;
			this.timestamp = timestamp;
		}

	}
}

