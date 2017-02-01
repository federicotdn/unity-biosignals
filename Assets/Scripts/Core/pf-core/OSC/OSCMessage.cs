using System;
using System.Collections.Generic;
using System.Text;

namespace OSC
{
	[Serializable]
	public class OSCMessage : OSCPacket
	{
		public String Address { get; private set; }


		public OSCMessage(byte[] data)
		{
			int index = 0;

			Data = new List<Object>();
			Address = GetAddress(data, ref index);

			if (index % 4 != 0)
				throw new Exception("Misaligned OSC Packet data. Address string is not padded correctly and does not align to 4 byte interval");

			char[] types = getTypes(data, ref index);

			while (index % 4 != 0)
				index++;


			bool commaParsed = false;
			foreach (char type in types)
			{
				switch (type)
				{
					case ('\0'):
						break;
					case (','):
						if (commaParsed)
						{
							throw new Exception("OSC invalid format: A comma has already been parsed");
						}
						commaParsed = true;
						break;
					case ('i'):
						int intVal = UnpackInt32(data, ref index);
						Data.Add(intVal);
						break;

					case ('f'):
						float floatVal = UnpackFloat(data, ref index);
						Data.Add(floatVal);
						break;
					case ('s'):
						String s = UnpackString(data, ref index);
						Data.Add(s);
						break;
					case ('b'):
						byte[] blob = UnpackBlob(data, ref index);
						Data.Add(blob);
						break;
					case ('t'):
						Console.WriteLine("timetag");
						break;
					default:
						throw new Exception("Unsupported data type.");

				}
			}
		}

		public override bool IsBundle()
		{
			return false;
		}

		private static String GetAddress(byte[] data, ref int index)
		{
			int i = index;
			string address = "";
			for (; i < data.Length; i += 4)
			{
				if (data[i] == ',')
				{
					if (i == 0)
						return "";

					address = Encoding.ASCII.GetString(data.SubArray(index, i - 1));
					index = i;
					break;
				}
			}

			if (i >= data.Length && address == null)
				throw new Exception("no comma found");

			return address.Replace("\0", "");
		}

		private static char[] getTypes(byte[] data, ref int index)
		{
			int i = index + 4;
			char[] types = null;

			for (; i < data.Length; i += 4)
			{
				if (data[i - 1] == 0)
				{
					types = Encoding.ASCII.GetChars(data.SubArray(index, i - index));
					index = i;
					break;
				}
			}

			if (i >= data.Length && types == null)
				throw new Exception("No null terminator after type string");

			return types;
		}
	}
}