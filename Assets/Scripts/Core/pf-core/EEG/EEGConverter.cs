using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using OSC;
using System.Collections.Generic;

namespace pfcore
{
	public class EEGConverter
	{
		private string filePath;

		public EEGConverter(string filePath)
		{
			this.filePath = filePath;
		}

		public void Run()
		{
			FileStream stream = File.OpenRead(filePath);
			BinaryFormatter bin = new BinaryFormatter();
			List<OSCPacket> packets = (List<OSCPacket>)bin.Deserialize(stream);

			string filename = filePath.Split('.')[0];
			filename += "-v2.eeg";

			FileStream newStream = File.OpenWrite(filename);

			foreach (OSCPacket packet in packets)
			{
				//((OSCMessage)(packet.Data[0])).Extra = packet.Extra;
				byte[] bytes = packet.Pack();
				newStream.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
				newStream.Write(bytes, 0, bytes.Length);
			}
			stream.Close();
			newStream.Close();
			Console.WriteLine("Converted file saved to " + filename);
		}
	}
}
