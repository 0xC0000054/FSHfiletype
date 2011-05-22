using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using FSHLib;

namespace FSHfiletype
{
    internal static class BlendBitmap
    {
       /// <summary>
       /// Blends the fsh bitmap and alpha images
       /// </summary>
       /// <param name="bmpitem">The bitmap item to blend</param>
       /// <returns>The blended bitmap or null</returns>
       public static Bitmap BlendBmp(BitmapItem bmpitem)
       {
            Bitmap image = null;
            if (bmpitem.Bitmap != null && bmpitem.Alpha != null)
            {
                image = new Bitmap(bmpitem.Bitmap.Width, bmpitem.Bitmap.Height, PixelFormat.Format32bppArgb);

                Bitmap colorbmp = new Bitmap(bmpitem.Bitmap);
                Bitmap bmpalpha = new Bitmap(bmpitem.Alpha);
               
                BitmapData colordata = colorbmp.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                BitmapData alphadata = bmpalpha.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                BitmapData bdata = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                IntPtr scan0 = bdata.Scan0;
                try
                {
                    unsafe
                    {
                        byte* clrdata = (byte*)(void*)colordata.Scan0;
                        byte* aldata = (byte*)(void*)alphadata.Scan0;
                        byte* destdata = (byte*)(void*)scan0;
                        int offset = bdata.Stride - image.Width * 4;
                        int clroffset = colordata.Stride - image.Width * 4;
                        int aloffset = alphadata.Stride - image.Width * 4;
                        for (int y = 0; y < image.Height; y++)
                        {
                            for (int x = 0; x < image.Width; x++)
                            {
                                destdata[3] = aldata[0];
                                destdata[0] = clrdata[0];
                                destdata[1] = clrdata[1];
                                destdata[2] = clrdata[2];

                                destdata += 4;
                                clrdata += 4;
                                aldata += 4;
                            }
                            destdata += offset;
                            clrdata += clroffset;
                            aldata += aloffset;
                        }

                    }
                }
                finally
                {
                    colorbmp.UnlockBits(colordata);
                    bmpalpha.UnlockBits(alphadata);
                    image.UnlockBits(bdata);

                    colorbmp.Dispose();
                    bmpalpha.Dispose();
                }
                               
                return image;
            }
            else
            {
                return null;
            }
       }
    }
}
