using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;

namespace FSHfiletype
{
	[Serializable]
	internal sealed class FshMetadata
	{
		private byte[] dirName;
		private MipData mipData;
		private ushort[] misc;
		private bool entryCompressed;
		private List<FSHAttachment> attachments;

		public byte[] DirName
		{
			get
			{
				return dirName;
			}
		}

		public MipData MipData
		{
			get
			{
				return mipData;
			}
		}
		
		public ushort[] Misc
		{
			get
			{
				return misc;
			}
		}

		public bool EntryCompressed
		{
			get
			{
				return entryCompressed;
			}
		}

		public List<FSHAttachment> Attachments
		{
			get
			{
				return attachments;
			}
		}

		public void SetCurrentLayerSize(Size layerSize)
		{
			this.mipData.layerWidth = layerSize.Width;
			this.mipData.layerHeight = layerSize.Height;
		}

		public FshMetadata(byte[] dirName, int mipCount, bool mipPadding, ushort[] misc, bool compressed, List<FSHAttachment> attach)
		{
			this.dirName = dirName;
			this.mipData = new MipData(mipCount, mipPadding, new Size(0, 0));
			this.misc = misc;
			this.entryCompressed = compressed;
			this.attachments = attach;
		}   

		public FshMetadata(byte[] name, Size layerSize)
		{
			this.dirName = name;
			this.mipData = new MipData(0, false, layerSize);
			this.misc = null;
			this.entryCompressed = false;
			this.attachments = null;
		}

		private FshMetadata ()
		{
			this.dirName = null;
			this.mipData = new MipData();
			this.misc = null;
			this.entryCompressed = false;
			this.attachments = null;
		}

		public static FshMetadata FromEncodedString(string data, Size size)
		{
			string[] val = data.Split(',');

			FshMetadata metaData = new FshMetadata();

			metaData.dirName = Encoding.ASCII.GetBytes(val[0]);
			metaData.mipData = new MipData(int.Parse(val[1], CultureInfo.InvariantCulture), bool.Parse(val[2]), size);

			string[] miscStr = val[3].Split('_');

			ushort[] miscData = new ushort[4] { 
				ushort.Parse(miscStr[0], CultureInfo.InvariantCulture),
				ushort.Parse(miscStr[1], CultureInfo.InvariantCulture),
				ushort.Parse(miscStr[2], CultureInfo.InvariantCulture),
				ushort.Parse(miscStr[3], CultureInfo.InvariantCulture)
			};

			metaData.misc = miscData;
			metaData.entryCompressed = bool.Parse(val[4]);

			if (!string.IsNullOrEmpty(val[5]))
			{
				string[] attach = val[5].Split(':');

				metaData.attachments = new List<FSHAttachment>(attach.Length);

				foreach (var item in attach)
				{
					metaData.attachments.Add(new FSHAttachment(item));
				}
			}

			return metaData;
		}
	}
  
	[Serializable]
	internal struct MipData
	{
		public int count;
		public bool hasPadding;
		[NonSerialized]
		public int layerWidth;
		[NonSerialized]
		public int layerHeight;

		public MipData(int count, bool padded, Size size)
		{
			this.count = count;
			this.hasPadding = padded;
			this.layerHeight = size.Height;
			this.layerWidth = size.Width;
		}
	}

	[Serializable]
	internal sealed class FSHAttachment : ISerializable
	{
		public FSHEntryHeader header;
		public byte[] data;
		public bool isBinary;


		internal FSHAttachment(FSHEntryHeader head, byte[] bytes, bool binData)
		{
			if (binData)
			{
				this.header = new FSHEntryHeader() { code = -1};
			}
			else
			{
				this.header = head;
			} 
			this.data = bytes;
			this.isBinary = binData;
		}

		internal FSHAttachment(string value)
		{
			string[] data = value.Split('|');

			this.header = new FSHEntryHeader() { misc = new ushort[4] };

			if (!string.IsNullOrEmpty(data[0]))
			{
				string[] head = data[0].Split('_');

				this.header.code = int.Parse(head[0], CultureInfo.InvariantCulture);
				this.header.width = ushort.Parse(head[1], CultureInfo.InvariantCulture);
				this.header.height = ushort.Parse(head[2], CultureInfo.InvariantCulture);
				this.header.misc[0] = ushort.Parse(head[3], CultureInfo.InvariantCulture);
				this.header.misc[1] = ushort.Parse(head[4], CultureInfo.InvariantCulture);
				this.header.misc[2] = ushort.Parse(head[5], CultureInfo.InvariantCulture);
				this.header.misc[3] = ushort.Parse(head[6], CultureInfo.InvariantCulture);
			}

			this.data = Convert.FromBase64String(data[1]);
		}

		private FSHAttachment(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}

			this.header = (FSHEntryHeader)info.GetValue("header", typeof(FSHEntryHeader));
			this.data = (byte[])info.GetValue("data", typeof(byte[]));
			this.isBinary = info.GetBoolean("isBinary");
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("header", this.header, typeof(FSHEntryHeader));
			info.AddValue("data", this.data, typeof(byte[]));
			info.AddValue("isBinary", this.isBinary);
		}
	}  

}
