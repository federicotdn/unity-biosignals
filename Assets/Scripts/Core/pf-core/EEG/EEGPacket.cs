using System;

namespace pfcore
{

	public enum DataType {
		ALPHA
	}

	public class EEGPacket
	{
		public DataType Type { get; }
		public float[] Data { get; }


		public EEGPacket (DataType type, float[] data) {
			this.Type = type;
			this.Data = data;
		}

	}
}

