using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ritsukage.Tools
{ 
	public class ByteDataBuilder
	{
		private readonly List<byte> data = new List<byte>();

		public int WritePointer = 0;

		public int Length
		{
			get
			{
				return data.Count;
			}
		}

		public void Write(short value, bool overwrite = true) => Write(GetBytes(value), overwrite);
		public void Write(int value, bool overwrite = true) => Write(GetBytes(value), overwrite);
		public void Write(long value, bool overwrite = true) => Write(GetBytes(value), overwrite);
		public void Write(string value, bool overwrite = true) => Write(GetBytes(value), overwrite);

		public byte[] GetData() => data.ToArray<byte>();

		private void Write(byte[] bs, bool overwrite = true)
		{
			for (int i = 0; i < bs.Length; ++i)
			{
				if (overwrite && WritePointer < data.Count)
					data[WritePointer++] = bs[i];
				else
					data.Insert(WritePointer++, bs[i]);
			}
		}

		private byte[] GetBytes(short value)
		{
			byte[] bs = new byte[2];
			bs[0] = (byte)((value >> 0) & 0xff);
			bs[1] = (byte)((value >> 8) & 0xff);
			return bs;
		}

		private byte[] GetBytes(int value)
		{
			byte[] bs = new byte[4];
			bs[0] = (byte)((value >> 0) & 0xff);
			bs[1] = (byte)((value >> 8) & 0xff);
			bs[2] = (byte)((value >> 16) & 0xff);
			bs[3] = (byte)((value >> 24) & 0xff);
			return bs;
		}

		private byte[] GetBytes(long value)
		{
			byte[] bs = new byte[8];
			bs[0] = (byte)((value >> 0) & 0xff);
			bs[1] = (byte)((value >> 8) & 0xff);
			bs[2] = (byte)((value >> 16) & 0xff);
			bs[3] = (byte)((value >> 24) & 0xff);
			bs[4] = (byte)((value >> 32) & 0xff);
			bs[5] = (byte)((value >> 40) & 0xff);
			bs[6] = (byte)((value >> 48) & 0xff);
			bs[7] = (byte)((value >> 56) & 0xff);
			return bs;
		}

		private byte[] GetBytes(string value)
		{
			return Encoding.UTF8.GetBytes(value);
		}
	}
}
