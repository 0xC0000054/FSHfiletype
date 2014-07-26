using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using FSHfiletype.Properties;
using PaintDotNet;

namespace FSHfiletype
{
    class FshFile
    {
        public FshFile()
        {
            useFshwriteComp = false;
        }

        private const string fshMetadata = "FshData";

        public Document Load(Stream input)
        {
            try
            {
                using (FshImageLoad image = new FshImageLoad(input))
                {
                    Document doc = new Document(image.Bitmaps[0].Surface.Width, image.Bitmaps[0].Surface.Height);
                    string fshName = Resources.FshLayerTitle;
                    int count = image.Bitmaps.Count;
                    for (int i = 0; i < count; i++)
                    {
                        FshLoadBitmapItem bmpitem = image.Bitmaps[i];


                        BitmapLayer bl = new BitmapLayer(bmpitem.Surface)
                        {
                            Name = fshName + i.ToString(),
                            IsBackground = (i == 0)
                        };



                        bl.Metadata.SetUserValue(fshMetadata, bmpitem.MetaData.ToString());

                        doc.Layers.Add(bl);
                    }

                    return doc;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool useFshwriteComp;

        private static int GetBmpDataSize(int width, int height, FshFileFormat format)
        {
            int ret = -1;
            switch (format)
            {
                case FshFileFormat.TwentyFourBit:
                    ret = (width * height * 3);
                    break;
                case FshFileFormat.ThirtyTwoBit:
                    ret = (width * height * 4);
                    break;
                case FshFileFormat.DXT1:
                    ret = (width * height / 2);
                    break;
                case FshFileFormat.DXT3:
                    ret = (width * height);
                    break;
                case FshFileFormat.SixteenBit:
                case FshFileFormat.SixteenBitAlpha:
                case FshFileFormat.SixteenBit4x4:
                    ret = (width * height * 2);
                    break;
            }
            return ret;
        }

        /// <summary>
        /// Saves the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <param name="token">The token.</param>
        /// <param name="scratchSurface">The scratch surface.</param>
        public unsafe void Save(Document input, Stream output, PropertyBasedSaveConfigToken token, Surface scratchSurface)
        {
            try
            {

                this.useFshwriteComp = (bool)token.GetProperty(PropertyNames.FshWriteCompression).Value;
                FshFileFormat format = (FshFileFormat)token.GetProperty(PropertyNames.FileType).Value;
                string dirText = (string)token.GetProperty(PropertyNames.DirectoryName).Value;

                if (dirText.Length < 4)
                {
                    dirText = "0000";
                }

                int count = input.Layers.Count;
                List<FshMetadata> metaData = new List<FshMetadata>(count);
                Encoding ascii = Encoding.ASCII;
                byte[] dirName = ascii.GetBytes(dirText);

                for (int i = 0; i < count; i++)
                {
                    Layer item = (Layer)input.Layers[i];
                    string val = item.Metadata.GetUserValue(fshMetadata);
                    if (!string.IsNullOrEmpty(val))
                    {
                        metaData.Add(new FshMetadata(val, item.Size));
                    }
                    else
                    {
                        metaData.Add(new FshMetadata(dirName, item.Size));
                    }
                }

                //write header
                output.Write(ascii.GetBytes("SHPI"), 0, 4); // write SHPI id
                output.Write(BitConverter.GetBytes(0), 0, 4); // placeholder for the length
                output.Write(BitConverter.GetBytes(count), 0, 4); // write the number of bitmaps in the list
                output.Write(ascii.GetBytes("G264"), 0, 4); // header directory


                int fshlen = 16 + (8 * count); // fsh length
                int bmpw, bmph, attachCode;
                for (int i = 0; i < count; i++)
                {
                    FshMetadata meta = metaData[i];
                    output.Write(meta.DirName, 0, 4);
                    output.Write(BitConverter.GetBytes(fshlen), 0, 4);

                    MipData mips = meta.MipData;
                    for (int j = 0; j <= mips.count; j++)
                    {
                        bmpw = (mips.layerWidth >> j);
                        bmph = (mips.layerHeight >> j);

                        if (format == FshFileFormat.DXT1)
                        {
                            bmpw += (4 - bmpw) & 3; // 4x4 blocks
                            bmph += (4 - bmph) & 3;
                        }

                        int imageDataLength = GetBmpDataSize(bmpw, bmph, format);

                        if (!mips.hasPadding && format != FshFileFormat.DXT1 || mips.hasPadding && j == mips.count)
                        {
                            int padding = (16 - imageDataLength) & 15;
                            if (padding > 0)
                            {
                                imageDataLength += padding; // pad to a 16 byte boundary
                            }
                        }

                        fshlen += imageDataLength;
                    }
                    
                    if (meta.Attachments != null)
                    {
                        int dataLen;
                        foreach (FSHAttachment item in meta.Attachments)
                        {
                            dataLen = item.data.Length;

                            if (item.isBinary)
                            {
                                fshlen += dataLen;
                            }
                            else
                            { 
                                attachCode = item.header.code & 0xff;
                                switch (attachCode)
                                {
                                    case 0x6f: // TXT
                                        fshlen += (8 + dataLen);
                                        break;
                                    case 0x69: // ETXT full header
                                        fshlen += (16 + dataLen);
                                        break;
                                    case 0x70: // ETXT 16 bytes
                                        fshlen += 16;
                                        break;
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < count; i++)
                {
                    BitmapLayer bl = (BitmapLayer)input.Layers[i];

                    // write the entry header
                    int code = 0;

                    switch (format)
                    {
                        case FshFileFormat.TwentyFourBit:
                            code = 0x7f;
                            break;
                        case FshFileFormat.ThirtyTwoBit:
                            code = 0x7d;
                            break;
                        case FshFileFormat.SixteenBit:
                            code = 0x78;
                            break;
                        case FshFileFormat.SixteenBitAlpha:
                            code = 0x7e;
                            break;
                        case FshFileFormat.SixteenBit4x4:
                            code = 0x6d;
                            break;
                        case FshFileFormat.DXT1:
                            code = 0x60;
                            break;
                        case FshFileFormat.DXT3:
                            code = 0x61;
                            break;
                    }


                    long entryStart = output.Position;

                    output.Write(BitConverter.GetBytes(code), 0, 4);
                    output.Write(BitConverter.GetBytes((ushort)bl.Width), 0, 2);
                    output.Write(BitConverter.GetBytes((ushort)bl.Height), 0, 2);

                    FshMetadata meta = metaData[i];

                    MipData mip = meta.MipData;
                    ushort[] misc = meta.Misc;

                    if (misc == null)
                    {
                        misc = new ushort[4] { 0, 0, 0, 0 };
                    }

                    for (int j = 0; j < 4; j++)
                    {
                        output.Write(BitConverter.GetBytes(misc[j]), 0, 2);
                    }

                    int width = bl.Width;
                    int height = bl.Height;
                    int dataLen = 0;
                    Surface src = bl.Surface;
                    Surface surf = src;

                    bool compressed = false;

                    int realWidth, realHeight;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        for (int j = 0; j <= mip.count; j++)
                        {
                            bmpw = realWidth = (width >> j);
                            bmph = realHeight = (height >> j);

                            if (format == FshFileFormat.DXT1) // Maxis files use this
                            {
                                bmpw += (4 - bmpw) & 3; // 4x4 blocks 
                                bmph += (4 - bmph) & 3;
                            }

                            if (j > 0)
                            {
                                surf = new Surface(bmpw, bmph);

                                if (format == FshFileFormat.DXT1 && (realWidth < bmpw || realHeight < bmph))
                                {
                                    // the 2x2 and smaller bitmaps are padded to 4x4 with transparent pixels 
                                    using (Surface temp = new Surface(realWidth, realHeight))
                                    {
                                        temp.FitSurface(ResamplingAlgorithm.SuperSampling, src);
                                        surf.CopySurface(temp);
                                    }
                                }
                                else
                                {
                                    surf.FitSurface(ResamplingAlgorithm.SuperSampling, src);
                                }
                            }

                            dataLen = GetBmpDataSize(bmpw, bmph, format);

                            byte[] data = SaveImageData(surf, format, dataLen);


                            if (!mip.hasPadding && format != FshFileFormat.DXT1 || mip.hasPadding && j == mip.count)
                            {
                                while ((dataLen & 15) > 0)
                                {
                                    data[dataLen++] = 0; // pad to a 16 byte boundary
                                }
                            }

                            ms.Write(data, 0, dataLen);
                        }

                        if (meta.EntryCompressed)
                        {
                            byte[] comp = QfsComp.Comp(ms.ToArray(), false);

                            if (comp != null)
                            {
                                output.Write(comp, 0, comp.Length);
                                code |= 0x80;
                                compressed = true;
                            }
                            else
                            {
                                ms.WriteTo(output);
                            }
                        }
                        else
                        {
                            ms.WriteTo(output);
                        }

                    }

                    if (mip.count > 0 || compressed)
                    {
                        long newPosition = output.Position;
                        long sectionLength = newPosition - entryStart;
                        int newCode = (((int)sectionLength << 8) | code);

                        output.Seek(entryStart, SeekOrigin.Begin);
                        output.Write(BitConverter.GetBytes(newCode), 0, 4);
                        output.Seek(newPosition, SeekOrigin.Begin);
                    }

                    List<FSHAttachment> attach = meta.Attachments;
                    if (attach != null)
                    {
                        foreach (FSHAttachment item in attach)
                        {
                            if (item.data.Length > 0)
                            {
                                if (item.isBinary)
                                {
                                    output.Write(item.data, 0, item.data.Length);
                                }
                                else
                                {
                                    attachCode = item.header.code & 0xff;

                                    output.Write(BitConverter.GetBytes(item.header.code), 0, 4);

                                    if (attachCode != 0x70)
                                    {
                                        output.Write(BitConverter.GetBytes(item.header.width), 0, 2);
                                        output.Write(BitConverter.GetBytes(item.header.height), 0, 2);
                                    }

                                    switch (attachCode)
                                    {
                                        case 0x6f: // TXT
                                        case 0x70: // ETXT 16 bytes
                                            output.Write(item.data, 0, item.data.Length);
                                            break;
                                        case 0x69: // ETXT full header
                                            for (int m = 0; m < 4; m++)
                                            {
                                                output.Write(BitConverter.GetBytes(item.header.misc[m]), 0, 2);
                                            }
                                            output.Write(item.data, 0, item.data.Length);
                                            break;
                                    }
                                } 
                            }
                        }
                    }
                }

                output.Seek(4L, SeekOrigin.Begin);
                output.Write(BitConverter.GetBytes(output.Length), 0, 4);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Resources.ErrorSavingCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private unsafe byte[] SaveImageData(Surface surf, FshFileFormat format, int dataLength)
        {
            int width = surf.Width;
            int height = surf.Height;
            byte[] data = null;

            if (format != FshFileFormat.DXT1 && format != FshFileFormat.DXT3)
            {
                data = new byte[dataLength + 2000];
            }

            if (format == FshFileFormat.TwentyFourBit)
            {
                fixed (byte* ptr = data)
                {
                    int dstStride = width * 3;
                    for (int y = 0; y < height; y++)
                    {
                        ColorBgra* src = surf.GetRowAddressUnchecked(y);
                        byte* dst = ptr + (y * dstStride);
                        for (int x = 0; x < width; x++)
                        {
                            dst[0] = src->B;
                            dst[1] = src->G;
                            dst[2] = src->R;

                            src++;
                            dst += 3;
                        }
                    }
                }
            }
            else if (format == FshFileFormat.ThirtyTwoBit)
            {
                fixed (byte* ptr = data)
                {
                    int dstStride = width * 4;
                    for (int y = 0; y < height; y++)
                    {
                        ColorBgra* src = surf.GetRowAddressUnchecked(y);
                        byte* dst = ptr + (y * dstStride);
                        for (int x = 0; x < width; x++)
                        {
                            dst[0] = src->B;
                            dst[1] = src->G;
                            dst[2] = src->R;
                            dst[3] = src->A;

                            src++;
                            dst += 4;
                        }
                    }
                }
            }
            else if (format == FshFileFormat.DXT1)
            {
                if (useFshwriteComp)
                {
                    int flags = 0;
                    flags |= (int)Squish.SquishFlags.kDxt1;
                    flags |= (int)Squish.SquishFlags.kColourIterativeClusterFit;
                    flags |= (int)Squish.SquishFlags.kColourMetricPerceptual;
                    data = Squish.CompressImage(surf, flags);
                }
                else
                {
                    data = DXTComp.CompressFSHToolDXT1((byte*)surf.Scan0.VoidStar, width, height);
                }
            }
            else if (format == FshFileFormat.DXT3)
            {
                if (useFshwriteComp)
                {
                    int flags = 0;
                    flags |= (int)Squish.SquishFlags.kDxt3;
                    flags |= (int)Squish.SquishFlags.kColourIterativeClusterFit;
                    flags |= (int)Squish.SquishFlags.kColourMetricPerceptual;
                    data = Squish.CompressImage(surf, flags);
                }
                else
                {
                    data = DXTComp.CompressFSHToolDXT3((byte*)surf.Scan0.VoidStar, width, height);
                }
            }
            else if (format == FshFileFormat.SixteenBit)
            {
                fixed (byte* ptr = data)
                {
                    for (int y = 0; y < height; y++)
                    {
                        ColorBgra* src = surf.GetRowAddressUnchecked(y);
                        ushort* dst = (ushort*)ptr + (y * width);
                        for (int x = 0; x < width; x++)
                        {
                            dst[0] = (ushort)((src->B >> 3) + ((src->G >> 2) << 5) + ((src->R >> 3) << 11));

                            src++;
                            dst++;
                        }
                    }
                }
            }
            else if (format == FshFileFormat.SixteenBitAlpha)
            {
                fixed (byte* ptr = data)
                {
                    for (int y = 0; y < height; y++)
                    {
                        ColorBgra* src = surf.GetRowAddressUnchecked(y);
                        ushort* dst = (ushort*)ptr + (y * width);
                        for (int x = 0; x < width; x++)
                        {
                            dst[0] = (ushort)((src->B >> 3) + ((src->G >> 3) << 5) + ((src->R >> 3) << 10));

                            if (src->A >= 128)
                            {
                                dst[0] |= (ushort)0x8000;
                            }

                            src++;
                            dst++;
                        }
                    }
                }
            }
            else if (format == FshFileFormat.SixteenBit4x4)
            {
                fixed (byte* ptr = data)
                {
                    for (int y = 0; y < height; y++)
                    {
                        ColorBgra* src = surf.GetRowAddressUnchecked(y);
                        ushort* dst = (ushort*)ptr + (y * width);
                        for (int x = 0; x < width; x++)
                        {
                            dst[0] = (ushort)((src->B >> 4) + ((src->G >> 4) << 4) + ((src->R >> 4) << 8) + ((src->A >> 4) << 12));

                            src++;
                            dst++;
                        }
                    }
                }
            }

            return data;
        }


    }
}
