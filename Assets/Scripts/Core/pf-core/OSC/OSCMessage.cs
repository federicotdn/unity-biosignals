using System;
using System.Collections.Generic;
using System.Text;

namespace OSC
{
	[Serializable]
	public class OSCMessage : OSCPacket
	{
		public String Address { get; private set; }

		public byte Extra;

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
					case ('x'):
						int extra = UnpackInt32(data, ref index);
						Extra = (byte)extra;
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

		public override byte[] Pack()
		{
			List<byte[]> parts = new List<byte[]>();

			List<object> currentList = Data;

			string typeString = ",";
			int i = 0;
			while (i < currentList.Count)
			{
				var arg = currentList[i];

				string type = (arg != null) ? arg.GetType().ToString() : "null";
				switch (type)
				{
					case "System.Int32":
						typeString += "i";
						parts.Add(PackInt32((int)arg));
						break;
					case "System.Single":
						typeString += "f";
						parts.Add(PackFloat((float)arg));

						break;
					case "System.String":
						typeString += "s";
						parts.Add(PackString((string)arg));
						break;
					case "System.Byte[]":
						typeString += "b";
						parts.Add(PackBlob((byte[])arg));
						break;
					case "System.UInt64":
						typeString += "t";
						parts.Add(PackUInt64((UInt64)arg));
						break;
					default:
						throw new Exception("Unable to transmit values of type " + type);
				}

				i++;
			}

			// Add info about the eye status
			typeString += "x";
			parts.Add(PackInt32(Extra));

			int addressLen = (Address.Length == 0 || Address == null) ? 0 : AlignedStringLength(Address);
			int typeLen = AlignedStringLength(typeString);

			int total = addressLen + typeLen;

			foreach (byte[] b in parts)
			{
				total += b.Length;
			}

			byte[] output = new byte[total];
			i = 0;

			Encoding.ASCII.GetBytes(Address).CopyTo(output, i);
			i += addressLen;

			Encoding.ASCII.GetBytes(typeString).CopyTo(output, i);
			i += typeLen;

			foreach (byte[] part in parts)
			{
				part.CopyTo(output, i);
				i += part.Length;
			}

			return output;
		}
	}
}