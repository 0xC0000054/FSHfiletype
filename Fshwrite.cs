using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using PaintDotNet;

namespace FSHfiletype
{
    internal class Fshwrite
    {
        public Fshwrite()
        {
            bmpList = new List<Bitmap>();
            alphaList = new List<Bitmap>();
            dirnames = new List<byte[]>();
            codeList = new List<int>();
            headdir = null;
        }

        private Bitmap BlendDXTBmp(Bitmap color, Bitmap alpha)
        {
            Bitmap image = null;
            using (Bitmap temp = new Bitmap(color.Width, color.Height, PixelFormat.Format32bppArgb))
            {

                if (color.Size != alpha.Size)
                {
                    throw new ArgumentException("The bitmap and alpha must be equal size");
                }

                BitmapData colorData = color.LockBits(new Rectangle(0, 0, color.Width, color.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                BitmapData alphaData = alpha.LockBits(new Rectangle(0, 0, alpha.Width, alpha.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                BitmapData bdata = temp.LockBits(new Rectangle(0, 0, temp.Width, temp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                try
                {
                    unsafe
                    {

                        byte* colorScan0 = (byte*)colorData.Scan0.ToPointer();
                        byte* alphaScan0 = (byte*)alphaData.Scan0.ToPointer();
                        byte* destScan0 = (byte*)bdata.Scan0.ToPointer();
                        int colorStride = colorData.Stride;
                        int alphaStride = alphaData.Stride;
                        int destStride = bdata.Stride;

                        for (int y = 0; y < temp.Height; y++)
                        {
                            byte* clr = colorScan0 + (y * colorStride);
                            byte* al = alphaScan0 + (y * alphaStride);
                            byte* dst = destScan0 + (y * destStride); 

                            for (int x = 0; x < temp.Width; x++)
                            {

                                dst[0] = clr[0];
                                dst[1] = clr[1];
                                dst[2] = clr[2];
                                dst[3] = al[0];


                                clr += 3;
                                al += 3;       
                                dst += 4;
                            }
                        }

                    }
                }
                finally
                {
                    color.UnlockBits(colorData);
                    alpha.UnlockBits(alphaData);
                    temp.UnlockBits(bdata);
                }

                image = temp.CloneT<Bitmap>();
            }
            
            return image;
        }

        private List<Bitmap> bmpList = null;
        private List<Bitmap> alphaList = null;
        private List<byte[]> dirnames = null;
        private List<int> codeList = null;
        private byte[] headdir = null;
        private int GetBmpDataSize(Bitmap bmp, int code)
        {
            int ret = -1;
            switch (code)
            {
                case 0x60:
                    ret = (bmp.Width * bmp.Height / 2); //Dxt1
                    break;
                case 0x61:
                    ret = (bmp.Width * bmp.Height); //Dxt3
                    break;
            }
            return ret;
        }
        public List<Bitmap> alpha
        {
            get 
            {
                return alphaList;
            }
            set
            {
                alphaList = value;
            }
        }
        public byte[] HeadDir
        {
            get
            {
                return headdir;
            }
            set
            {
                headdir = value;
            }
        }
        public List<Bitmap> bmp
        {
            get
            {
                return bmpList;
            }
            set
            {
                bmpList = value;
            }
        }
        public List<byte[]> dir
        {
            get
            {
                return dirnames;
            }
            set
            {
                dirnames = value;
            }
        }
        public List<int> code
        {
            get
            {
                return codeList;
            }
            set
            {
                codeList = value;
            }
        }
        /// <summary>
        /// The function that writes the fsh
        /// </summary>
        /// <param name="output">The output file to write to</param>
        public unsafe void WriteFsh(Stream output)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                if (bmpList != null && bmpList.Count > 0 && alphaList != null && dirnames != null && codeList != null)
                {
                    //write header
                    ms.Write(Encoding.ASCII.GetBytes("SHPI"), 0, 4); // write SHPI id
                    ms.Write(BitConverter.GetBytes(0), 0, 4); // placeholder for the length
                    ms.Write(BitConverter.GetBytes(bmpList.Count), 0, 4); // write the number of bitmaps in the list
                    if (headdir != null)
                    {
                        ms.Write(headdir, 0, 4); // override the header directory id (eg. G315 - emergency lights)
                    }
                    else
                    {
                        ms.Write(Encoding.ASCII.GetBytes("G264"), 0, 4); // header directory
                    }
                    int fshlen = 16 + (8 * bmpList.Count); // fsh length
                    for (int c = 0; c < bmpList.Count; c++)
                    {
                        //write directory
                       // Debug.WriteLine("bmp = " + c.ToString() + " offset = " + fshlen.ToString());
                        ms.Write(dir[c], 0, 4); // directory id
                        ms.Write(BitConverter.GetBytes(fshlen), 0, 4); // Write the Entry offset 

                        fshlen += 16; // skip the entry header length
                        int bmplen = GetBmpDataSize(bmpList[c], codeList[c]);
                        fshlen += bmplen; // skip the bitmap length
                    }
                    for (int b = 0; b < bmpList.Count; b++)
                    {
                        Bitmap bmp = bmpList[b];
                        Bitmap alpha = alphaList[b];
                        int code = codeList[b];
                        // write entry header
                        ms.Write(BitConverter.GetBytes(code), 0, 4); // write the Entry bitmap code
                        ms.Write(BitConverter.GetBytes((ushort)bmp.Width), 0, 2); // write width
                        ms.Write(BitConverter.GetBytes((ushort)bmp.Height), 0, 2); //write height
                        for (int m = 0; m < 4; m++)
                        {
                            ms.Write(BitConverter.GetBytes((ushort)0), 0, 2);// write misc data
                        }
                        Bitmap temp = BlendDXTBmp(bmp, alpha);
                        byte[] data = new byte[temp.Width * temp.Height * 4];
                        int flags = 0;
                        
                        flags |= code == 0x60 ? (int)Squish.SquishFlags.kDxt1 : (int)Squish.SquishFlags.kDxt3;
                        flags |= (int)Squish.SquishFlags.kColourIterativeClusterFit;
                        flags |= (int)Squish.SquishFlags.kColourMetricPerceptual;
                        data = Squish.CompressImage(temp, flags);
                      
                        ms.Write(data, 0, data.Length);
                    }

                    ms.Position = 4L;
                    ms.Write(BitConverter.GetBytes((int)ms.Length), 0, 4); // write the files length
                    ms.WriteTo(output); // write the memory stream to the file
                }
            }
        }

    }
}
