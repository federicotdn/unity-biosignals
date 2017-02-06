using System;
using System.Collections.Generic;
using System.Text;

namespace OSC
{
	[Serializable]
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

		public abstract byte[] Pack();

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

		protected static byte[] PackUInt64(UInt64 val)
		{
			byte[] data = BitConverter.GetBytes(val);
			int aux = 0;
			if (BitConverter.IsLittleEndian) data = FixEndianness(data, ref aux, data.Length);
			return data;
		}

		protected static byte[] PackInt32(Int32 val)
		{
			byte[] data = BitConverter.GetBytes(val);
			int aux = 0;
			if (BitConverter.IsLittleEndian) data = FixEndianness(data, ref aux, data.Length);
			return data;
		}

		protected static byte[] PackFloat(float val)
		{
			byte[] data = BitConverter.GetBytes(val);
			int aux = 0;
			if (BitConverter.IsLittleEndian) data = FixEndianness(data, ref aux, data.Length);
			return data;
		}

		protected static byte[] PackBlob(byte[] bytes)
		{
			int len = bytes.Length + 4;
			len = len + (4 - len % 4);

			byte[] msg = new byte[len];
			byte[] size = PackInt32(bytes.Length);
			size.CopyTo(msg, 0);
			bytes.CopyTo(msg, 4);
			return msg;
		}

		protected static byte[] PackString(String val)
		{
			int len = val.Length + (4 - val.Length % 4);
			if (len <= val.Length) len = len + 4;

			byte[] msg = new byte[len];

			var bytes = Encoding.ASCII.GetBytes(val);
			bytes.CopyTo(msg, 0);

			return msg;
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

		protected static int AlignedStringLength(string val)
		{
			int len = val.Length + (4 - val.Length % 4);
			if (len <= val.Length) len += 4;

			return len;
		}
	}
}
