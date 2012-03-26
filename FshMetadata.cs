using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Globalization;

namespace FSHfiletype
{

    /// <summary>
    /// 
    /// </summary>
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
            internal set
            { 
            
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
            this.mipData = new MipData(mipCount, mipPadding, new Size(0, 0));
            this.misc = misc;
            this.entryCompressed = compressed;
            this.attachments = attach;
        }   
        
        public FshMetadata(string data, Size size)
        {
            string[] val = data.Split(',');

            this.dirName = Encoding.ASCII.GetBytes(val[0]);
            this.mipData = new MipData(int.Parse(val[1], CultureInfo.InvariantCulture), bool.Parse(val[2]), size);
            
            string[] miscStr = val[3].Split('_');

			ushort[] miscData = new ushort[4] { 
				ushort.Parse(miscStr[0], CultureInfo.InvariantCulture),
				ushort.Parse(miscStr[1], CultureInfo.InvariantCulture),
				ushort.Parse(miscStr[2], CultureInfo.InvariantCulture),
				ushort.Parse(miscStr[3], CultureInfo.InvariantCulture)
			};

            this.misc = miscData;
            this.entryCompressed = bool.Parse(val[4]);

            if (!string.IsNullOrEmpty(val[5]))
            {
                string[] attach = val[5].Split(':');

                this.attachments = new List<FSHAttachment>(attach.Length);

                foreach (var item in attach)
                {
                    this.attachments.Add(new FSHAttachment(item));
                }
            }
        }

        public FshMetadata(byte[] name, Size size)
        {
            this.dirName = name;
            this.mipData = new MipData(0, false, size);
            this.misc = null;
            this.entryCompressed = false;
            this.attachments = null;
        }


        public override string ToString()
        {
            string miscData = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}_{3}", new object[]{ misc[0].ToStringInvariant(),
							misc[1].ToString(CultureInfo.InvariantCulture), misc[2].ToString(CultureInfo.InvariantCulture), misc[3].ToStringInvariant() });

            string attach = string.Empty;
            if (this.attachments != null)
            {
                StringBuilder sb = new StringBuilder();
                int length = this.attachments.Count;
                int maxLen = length - 1;
                for (int i = 0; i < length; i++)
                {
                    FSHAttachment att = attachments[i];

                    sb.Append(att.ToString());

                    if (i < maxLen)
                    {
                        sb.Append(':');
                    }
                }

                attach = sb.ToString();
            }


            string data = string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5}", new object[] { Encoding.ASCII.GetString(dirName), 
							mipData.count.ToStringInvariant(), mipData.hasPadding.ToString(), miscData, entryCompressed.ToString(), attach});

            return data;
        }


        

      
    }
  
    internal struct MipData
    {
        public int count;
        public bool hasPadding;
        public int layerWidth;
        public int layerHeight;

        public MipData(int count, bool padded, Size size)
        {
            this.count = count;
            this.hasPadding = padded;
            this.layerHeight = size.Height;
            this.layerWidth = size.Width;
        }
    }

    internal struct FSHAttachment
    {
        public FSHEntryHeader header;
        public byte[] data;

        public override string ToString()
        {
            string head = string.Empty;
            if (header.code >= 0) // don't encode the header for the binary data case
            {
                object[] headObj = new object[7] { header.code.ToStringInvariant(), header.width.ToStringInvariant(),
                header.height.ToStringInvariant(), header.misc[0].ToStringInvariant(), header.misc[1].ToStringInvariant(),
                header.misc[2].ToStringInvariant(), header.misc[3].ToStringInvariant()
                };
                head = string.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}", headObj);
            }

            string b64 = Convert.ToBase64String(data, Base64FormattingOptions.None);


            return string.Format("{0}|{1}", head, b64);
        }

        internal FSHAttachment(string value)
	    {
            string[] data = value.Split('|');

            this.header = new FSHEntryHeader() {  misc = new ushort[4] };

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
	    }
    }  

}
