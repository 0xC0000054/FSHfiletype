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

        public FshMetadata(byte[] dirName, int mipCount, bool mipPadding, ushort[] misc, bool compressed)
        {
            this.dirName = dirName;
            this.mipData = new MipData(mipCount, mipPadding, new Size(0, 0));
            this.misc = misc;
            this.entryCompressed = compressed;
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
        }

        public FshMetadata(byte[] name, Size size)
        {
            this.dirName = name;
            this.mipData = new MipData(0, false, size);
            this.misc = null;
            this.entryCompressed = false;
        }

        public override string ToString()
        {
            string miscData = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}_{3}", new object[]{ misc[0].ToString(CultureInfo.InvariantCulture),
							misc[1].ToString(CultureInfo.InvariantCulture), misc[2].ToString(CultureInfo.InvariantCulture), misc[3].ToString(CultureInfo.InvariantCulture) });

            string data = string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4}", new object[] { Encoding.ASCII.GetString(dirName), 
							mipData.count.ToString(CultureInfo.InvariantCulture), mipData.hasPadding.ToString(), miscData, entryCompressed.ToString() });

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

}
