using System;
using System.Text;
using System.Collections.Generic;
namespace OSC
{
	[Serializable]
	public class OSCBundle : OSCPacket
	{
		public long TimeStamp { get; private set; }

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

			// Discard timetag
			UnpackUInt64(data, ref index);

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
	}
}
