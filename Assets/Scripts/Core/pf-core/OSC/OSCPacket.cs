using System;
using System.Collections.Generic;
namespace OSC
{
	public abstract class OSCPacket
	{
		public List<Object> Data { get; protected set; }

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
			float ans = BitConverter.ToSingle(bytes, index);
			index += 4;
			return ans;
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
