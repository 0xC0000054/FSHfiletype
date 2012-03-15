﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Drawing;

namespace FSHfiletype
{
    static class Squish
    {
        private sealed class Squish_32
        {
            [DllImport(@"Native.x86\PaintDotNet.Native.x86.dll")]
            public static extern unsafe void SquishCompressImage(byte* rgba, int width, int height, byte* blocks, int flags, [MarshalAs(UnmanagedType.FunctionPtr)]  ProgressFn pf);

            [DllImport(@"Native.x86\PaintDotNet.Native.x86.dll")]
            public static extern unsafe void SquishDecompressImage(byte* rgba, int width, int height, byte* blocks, int flags, [MarshalAs(UnmanagedType.FunctionPtr)]  ProgressFn pf);

            [DllImport(@"Native.x86\PaintDotNet.Native.x86.dll")]
            public static extern void SquishInitialize();
        }
        private delegate void ProgressFn(int workDone, int workTotal);

        private sealed class Squish_64
        {
            //"PaintDotNet.SystemLayer.Native.x64.dll" // Paint.NET 4 
            [DllImport(@"Native.x64\PaintDotNet.Native.x64.dll")]
            public static extern unsafe void SquishCompressImage(byte* rgba, int width, int height, byte* blocks, int flags, [MarshalAs(UnmanagedType.FunctionPtr)]  ProgressFn pf);
            [DllImport(@"Native.x64\PaintDotNet.Native.x64.dll")]
            public static extern unsafe void SquishDecompressImage(byte* rgba, int width, int height, byte* blocks, int flags, [MarshalAs(UnmanagedType.FunctionPtr)]  ProgressFn pf);
            [DllImport(@"Native.x64\PaintDotNet.Native.x64.dll")]
            public static extern void SquishInitialize();
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

        internal static byte[] CompressImage(Bitmap image, int flags)
        {
            byte[] pixelData = new byte[image.Width * image.Height * 4];


            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                unsafe
                {


                    fixed (byte* ptr = pixelData)
                    {
                        int width = image.Width;
                        int height = image.Height;
                        void* scan0 = data.Scan0.ToPointer();
                        int stride = data.Stride;
                        int dstStride = width * 4;

                        for (int y = 0; y < height; y++)
                        {
                            byte* p = (byte*)scan0 + (y * stride);
                            byte* dst = ptr + (y * dstStride);
                            for (int x = 0; x < width; x++)
                            {
                                pixelData[0] = p[2];
                                pixelData[1] = p[1];
                                pixelData[2] = p[0];
                                pixelData[3] = p[3];

                                p += 4;
                                dst += 4;
                            }
                        } 
                    }
                }
            }
            finally
            {
                image.UnlockBits(data);
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
        internal static void InitilizeSquish()
        {
            if (Is64bit())
            {
                Squish_64.SquishInitialize();
            }
            else
            {
                Squish_32.SquishInitialize();
            }
        }
       
        private static unsafe void CompressImageWrapper(byte[] rgba, int width, int height, byte[] blocks, int flags)
        {
            fixed (byte* RGBA = rgba)
            {
                fixed (byte* Blocks = blocks)
                {
                    if (Is64bit())
                    {
                        Squish_64.SquishCompressImage(RGBA, width, height, Blocks, flags, null);
                    }
                    else
                    {
                        Squish_32.SquishCompressImage(RGBA, width, height, Blocks, flags, null);
                    }
                }
            }
        }

        private unsafe static void CallDecompressImage(byte[] rgba, int width, int height, byte[] blocks, int flags)
        {
            fixed (byte* pRGBA = rgba)
            {
                fixed (byte* pBlocks = blocks)
                {
                    if (Is64bit())
                    {
                        Squish_64.SquishDecompressImage(pRGBA, width, height, pBlocks, flags, null);
                    }
                    else
                    {
                        Squish_32.SquishDecompressImage(pRGBA, width, height, pBlocks, flags, null);
                    }
                }
            }
        }

        internal static byte[] DecompressImage(byte[] blocks, int width, int height, int flags)
        {
            byte[] rgba = new byte[(width * height) * 4];

            CallDecompressImage(rgba, width, height, blocks, flags);

            return rgba;
        }
    }
}
