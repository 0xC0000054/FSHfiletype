using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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

                        string base64MetaData = string.Empty;
                        using (MemoryStream stream = new MemoryStream())
                        {
                            new BinaryFormatter().Serialize(stream, bmpitem.MetaData);
                            base64MetaData = Convert.ToBase64String(stream.GetBuffer());
                        }

                        bl.Metadata.SetUserValue(fshMetadata, base64MetaData);

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

                int layerCount = input.Layers.Count;
                List<FshMetadata> metaData = new List<FshMetadata>(layerCount);
                byte[] dirName = Encoding.ASCII.GetBytes(dirText);

                for (int i = 0; i < layerCount; i++)
                {
                    Layer layer = (Layer)input.Layers[i];
                    string encodedMetaData = layer.Metadata.GetUserValue(fshMetadata);
                    if (!string.IsNullOrEmpty(encodedMetaData))
                    {
                        if (encodedMetaData.Contains(","))
                        {
                            metaData.Add(FshMetadata.FromEncodedString(encodedMetaData));
                        }
                        else
                        {
                            BinaryFormatter formatter = new BinaryFormatter() { Binder = new SelfBinder() };

                            byte[] data = Convert.FromBase64String(encodedMetaData);

                            using (MemoryStream stream = new MemoryStream(data))
                            {
                                FshMetadata meta = (FshMetadata)formatter.Deserialize(stream);

                                metaData.Add(meta);
                            }
                        }
                    }
                    else
                    {
                        metaData.Add(new FshMetadata(dirName));
                    }
                }

                //write header
                FSHHeader header = new FSHHeader(layerCount, "G264");
                header.Save(output);

                FSHDirEntry[] dirs = new FSHDirEntry[layerCount];

                long directoryStart = output.Position;

                for (int i = 0; i < layerCount; i++)
                {
                    // Write a placeholder for the directories, the real offsets will be written after the images have been saved. 
                    dirs[i] = new FSHDirEntry(metaData[i].DirName);
                    dirs[i].Save(output);
                }

                int bmpw, bmph, attachCode;

                for (int i = 0; i < layerCount; i++)
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

                    dirs[i].offset = (int)entryStart;

                    output.WriteInt32(code);
                    output.WriteUInt16((ushort)bl.Width);
                    output.WriteUInt16((ushort)bl.Height);

                    FshMetadata meta = metaData[i];

                    MipData mip = meta.MipData;
                    ushort[] misc = meta.Misc;

                    if (misc == null)
                    {
                        misc = new ushort[4] { 0, 0, 0, 0 };
                    }

                    for (int j = 0; j < 4; j++)
                    {
                        output.WriteUInt16(misc[j]);
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
                            byte[] comp = QfsComp.Compress(ms.GetBuffer(), false);

                            if (comp != null && comp.Length < ms.Length)
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

                    // Write the section length if the entry has mipmaps, is compressed or has attachments.
                    if (mip.count > 0 || compressed || meta.Attachments != null)
                    {
                        long newPosition = output.Position;
                        long sectionLength = newPosition - entryStart;
                        int newCode = (((int)sectionLength << 8) | code);

                        output.Seek(entryStart, SeekOrigin.Begin);
                        output.WriteInt32(newCode);
                        output.Seek(newPosition, SeekOrigin.Begin);
                    }

                    // write any attachments
                    if (meta.Attachments != null)
                    {
                        foreach (FSHAttachment item in meta.Attachments)
                        {                                

                            if (item.isBinary)
                            {
                                if (item.data.Length > 0)
                                {
                                    output.WriteInt32(item.header.code);
                                    output.Write(item.data, 0, item.data.Length);
                                }
                            }
                            else
                            {
                                output.WriteInt32(item.header.code);
                                attachCode = item.header.code & 0xff;

                                if (attachCode != 0x70)
                                {
                                    output.WriteUInt16(item.header.width);
                                    output.WriteUInt16(item.header.height);
                                }
                                
                                if (attachCode == 0x69 || attachCode == 0x7c)
                                {
                                    for (int m = 0; m < 4; m++)
                                    {
                                        output.WriteUInt16(item.header.misc[m]);
                                    }
                                }

                                switch (attachCode)
                                {
                                    case 0x6f: // TXT
                                    case 0x70: // ETXT 16 bytes
                                        output.Write(item.data, 0, item.data.Length);
                                        break;
                                    case 0x69: // ETXT full header
                                        output.Write(item.data, 0, item.data.Length);
                                        break;
                                    case 0x7c: // Pixel region, this only uses the misc fields in the header.
                                        break;
                                }
                            }

                        }
                    }

                }

                // Write the final length and directory offset.

                header.Size = (int)output.Length;
                
                output.Position = 0L;

                header.Save(output);

                output.Seek(directoryStart, SeekOrigin.Begin);

                for (int i = 0; i < dirs.Length; i++)
                {
                    dirs[i].Save(output);
                }
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

        /// <summary>
        /// Binds the serialization to types in the currently loaded assembly. 
        /// </summary>
        private sealed class SelfBinder : System.Runtime.Serialization.SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                return Type.GetType(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1}", typeName, assemblyName));
            }
        }
    }
}
