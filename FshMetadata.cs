/*
*  This file is part of fsh-filetype, a filetype plug-in for Paint.NET
*  that loads and saves FSH images.
*
*  Copyright (C) 2009, 2010, 2011, 2012, 2014, 2015, 2023 Nicholas Hayes
*
*  This program is free software: you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation, either version 3 of the License, or
*  (at your option) any later version.
*
*  This program is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with this program.  If not, see <http://www.gnu.org/licenses/>.
*
*/

using System;
using System.Collections.Generic;
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

		public FshMetadata(byte[] dirName, int mipCount, bool mipPadding, ushort[] misc, bool compressed, List<FSHAttachment> attach)
		{
			this.dirName = dirName;
			this.mipData = new MipData(mipCount, mipPadding);
			this.misc = misc;
			this.entryCompressed = compressed;
			this.attachments = attach;
		}   

		public FshMetadata(byte[] name)
		{
			this.dirName = name;
			this.mipData = new MipData(0, false);
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

		public static FshMetadata FromEncodedString(string data)
		{
			string[] val = data.Split(',');

			FshMetadata metaData = new FshMetadata();

			metaData.dirName = Encoding.ASCII.GetBytes(val[0]);
			metaData.mipData = new MipData(int.Parse(val[1], CultureInfo.InvariantCulture), bool.Parse(val[2]));

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

		public MipData(int count, bool padded)
		{
			this.count = count;
			this.hasPadding = padded;
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
			this.header = head;
			this.data = bytes;
			this.isBinary = binData;
		}

		internal FSHAttachment(string value)
		{
			string[] data = value.Split('|');

			this.header = new FSHEntryHeader();

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
