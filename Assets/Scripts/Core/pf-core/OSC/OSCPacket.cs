using System;
using System.Collections.Generic;
using System.Text;

namespace OSC
{
	[Serializable]
	public abstract class OSCPacket
	{
		public List<Object> Data { get; protected set; }
		public byte Extra;

		public abstract bool IsBundle();

		public static OSCPacket Unpack(byte[] data)
		{
			if (data[0] == '#')
			{
				return new OSCBundle(data);
			}
			else
			{
				return new OSCMessage(data);
			}
		}

		protected static UInt64 UnpackUInt64(byte[] bytes, ref int index)
		{
			int aux = index;
			byte[] data = FixEndianness(bytes, ref aux, sizeof(UInt64));
			UInt64 ans = BitConverter.ToUInt64(data, aux);
			index += sizeof(UInt64);
			return ans;
		}

		protected static Int32 UnpackInt32(byte[] bytes, ref int index)
		{
			int aux = index;
			byte[] data = FixEndianness(bytes, ref aux, sizeof(Int32));
			Int32 ans = BitConverter.ToInt32(data, aux);
			index += sizeof(Int32);
			return ans;
		}

		protected static float UnpackFloat(byte[] bytes, ref int index)
		{
			int aux = index;
			byte[] data = FixEndianness(bytes, ref aux, sizeof(float));
			float ans = BitConverter.ToSingle(data, aux);
			index += sizeof(float);
			return ans;
		}

		protected static byte[] UnpackBlob(byte[] bytes, ref int index)
		{
			int size = UnpackInt32(bytes, ref index);
			byte[] ans = bytes.SubArray(index, size);
			index += size;
			return ans;
		}

		protected static String UnpackString(byte[] bytes, ref int index)
		{
			String s = null;
			for (int i = index + 4; i < bytes.Length; i += 4)
			{
				if (bytes[i] == 0)
				{
					s = Encoding.ASCII.GetString(bytes.SubArray(index, i - index));
					s.Replace("\0", "");
					index = i;
					return s;
				}
			}

			throw new Exception("No null terminator after type string");
		}

		private static byte[] FixEndianness(byte[] bytes, ref int start, int size)
		{
			if (BitConverter.IsLittleEndian)
			{
				byte[] subArray = bytes.SubArray(start, size);
				Array.Reverse(subArray);
				start = 0;
				return subArray;
			}
			return bytes;
		}
	}
}
