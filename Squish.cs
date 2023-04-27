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
using System.Runtime.InteropServices;
using PaintDotNet;

namespace FSHfiletype
{
    static class Squish
    {
        [System.Security.SuppressUnmanagedCodeSecurity]
        private static class Squish_32
        {
            [DllImport("squish_Win32.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
            public static extern unsafe void CompressImage(byte* rgba, int width, int height, byte* blocks, int flags);
        }
        [System.Security.SuppressUnmanagedCodeSecurity]
        private static class Squish_64
        {
            [DllImport("squish_x64.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
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
            byte[] pixelData = new byte[(image.Width * image.Height * 4)];

                
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

            // Allocate room for compressed blocks with some padding
            byte[] blockData = new byte[(blockCount * blockSize) + 1024];

            // Invoke squish::CompressImage() with the required parameters
            CompressImageWrapper(pixelData, image.Width, image.Height, blockData, flags);

            // Return our block data to caller..
            return blockData;
        }
       
        private static unsafe void CompressImageWrapper(byte[] rgba, int width, int height, byte[] blocks, int flags)
        {
            fixed (byte* RGBA = rgba)
            {
                fixed (byte* Blocks = blocks)
                {
                    if (IntPtr.Size == 8)
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
