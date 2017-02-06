using System;
using System.Text;
using System.Collections.Generic;
namespace OSC
{
	[Serializable]
	public class OSCBundle : OSCPacket
	{
		public long TimeStamp { get; private set; }

		private UInt64 timetag;

		public OSCBundle(byte[] data)
		{
			TimeStamp = DateTime.Now.Ticks;

			int index = 0;

			String bundleTag = Encoding.ASCII.GetString(data.SubArray(0, 8));

			if (bundleTag != "#bundle\0")
			{
				throw new Exception("Not a bundle. Should start with '#bundle");
			}

			index += 8;

			timetag = UnpackUInt64(data, ref index);

			Data = new List<object>();

			while (index < data.Length)
			{
				int size = UnpackInt32(data, ref index);

				byte[] objectBytes = data.SubArray(index, size);
				OSCPacket packet = OSCPacket.Unpack(objectBytes);

				Data.Add(packet);

				index += size;
				while (index % 4 != 0)
					index++;
			}


		}

		public override bool IsBundle()
		{
			return true;
		}

		public override byte[] Pack()
		{
			string bundle = "#bundle";
			int bundleTagLen = AlignedStringLength(bundle);
			byte[] tag = PackUInt64(timetag);

			int len = bundleTagLen + tag.Length;

			List<byte[]> bytes = new List<byte[]>();
			foreach (OSCMessage msg in Data)
			{
				byte[] b = msg.Pack();
				bytes.Add(b);
				len += b.Length + 4;
			}

			int i = 0;
			byte[] output = new byte[len];
			Encoding.ASCII.GetBytes(bundle).CopyTo(output, i);
			i += bundleTagLen;
			tag.CopyTo(output, i);
			i += tag.Length;

			foreach (byte[] msg in bytes)
			{
				byte[] msgSize = PackInt32(msg.Length);
				msgSize.CopyTo(output, i);
				i += msgSize.Length;

				msg.CopyTo(output, i);
				i += msg.Length; // msg size is always a multiple of 4
			}



			return output;
		}
	}
}
