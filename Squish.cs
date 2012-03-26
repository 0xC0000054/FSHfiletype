using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Drawing;
using PaintDotNet;

namespace FSHfiletype
{
    static class Squish
    {
        [System.Security.SuppressUnmanagedCodeSecurity]
        private static class Squish_32
        {
            [DllImport(@"squish_Win32.dll")]
            public static extern unsafe void CompressImage(byte* rgba, int width, int height, byte* blocks, int flags);
        }
        [System.Security.SuppressUnmanagedCodeSecurity]
        private static class Squish_64
        {
            [DllImport(@"squish_x64.dll")]
            public static extern unsafe void CompressImage(byte* rgba, int width, int height, byte* blocks, int flags);
        }

        internal enum SquishFlags
        {
            //! Use DXT1 compression.
            kDxt1 = (1 << 0),

            //! Use DXT3 compression.
            kDxt3 = (1 << 1),

            //! Use DXT5 compression.
            kDxt5 = (1 << 2),

            //! Use a very slow but very high quality colour compressor.
            kColourIterativeClusterFit = (1 << 8),

            //! Use a slow but high quality colour compressor (the default).
            kColourClusterFit = (1 << 3),

            //! Use a fast but low quality colour compressor.
            kColourRangeFit = (1 << 4),

            //! Use a perceptual metric for colour error (the default).
            kColourMetricPerceptual = (1 << 5),

            //! Use a uniform metric for colour error.
            kColourMetricUniform = (1 << 6),

        }

        internal static unsafe byte[] CompressImage(Surface image, int flags)
        {
            byte[] pixelData = new byte[(image.Width * image.Height * 4) + 2000];

                
            fixed (byte* ptr = pixelData)
            {
                int width = image.Width;
                int height = image.Height;
                int dstStride = width * 4;

                for (int y = 0; y < height; y++)
                {
                    ColorBgra* p = image.GetRowAddressUnchecked(y);
                    byte* dst = ptr + (y * dstStride);
                    for (int x = 0; x < width; x++)
                    {
                        dst[0] = p->R;
                        dst[1] = p->G;
                        dst[2] = p->B;
                        dst[3] = p->A;

                        p++;
                        dst += 4;
                    }
                } 
            }
                
            // Compute size of compressed block area, and allocate 
            int blockCount = ((image.Width + 3) / 4) * ((image.Height + 3) / 4);
            int blockSize = ((flags & (int)SquishFlags.kDxt1) != 0) ? 8 : 16;

            // Allocate room for compressed blocks
            byte[] blockData = new byte[blockCount * blockSize];

            // Invoke squish::CompressImage() with the required parameters
            CompressImageWrapper(pixelData, image.Width, image.Height, blockData, flags);

            // Return our block data to caller..
            return blockData;
        }
        private static bool Is64bit()
        {
            return (IntPtr.Size == 8);
        }
       
        private static unsafe void CompressImageWrapper(byte[] rgba, int width, int height, byte[] blocks, int flags)
        {
            fixed (byte* RGBA = rgba)
            {
                fixed (byte* Blocks = blocks)
                {
                    if (Is64bit())
                    {
                        Squish_64.CompressImage(RGBA, width, height, Blocks, flags);
                    }
                    else
                    {
                        Squish_32.CompressImage(RGBA, width, height, Blocks, flags);
                    }
                }
            }
        }
    }
}
