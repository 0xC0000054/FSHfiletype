using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PaintDotNet;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Forms;
using FSHfiletype.Properties;
using System.Globalization;

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

						string data = string.Format(CultureInfo.InvariantCulture, "{0},{1},{2}", bmpitem.DirName, 
							bmpitem.EmbeddedMipCount.ToString(CultureInfo.InvariantCulture), bmpitem.MipPadding.ToString());

						bl.Metadata.SetUserValue(fshMetadata, data);
						
						doc.Layers.Add(bl);
					}

					return doc; 
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, Resources.ErrorLoadingCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return null;
			}
		}

		private bool useFshwriteComp;

		private int GetBmpDataSize(int width, int height, FshFileFormat format)
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
			}
			return ret;
		}

		private struct MipData
		{
			public int count;
			public bool hasPadding;
			public int layerWidth;
			public int layerHeight;

			public MipData(int count, bool padded, Size size)
			{
				this.count = count;
				this.hasPadding = padded;
				this.layerWidth = size.Width;
				this.layerHeight = size.Height;
			}
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
				bool alphaTrans = (bool)token.GetProperty(PropertyNames.AlphaFromTransparency).Value;
				FshFileFormat format = (FshFileFormat)token.GetProperty(PropertyNames.FileType).Value;
				string dirText = (string)token.GetProperty(PropertyNames.DirectoryName).Value;

				if (dirText.Length < 4)
				{
					dirText = "FiSH";
				}

				int count = input.Layers.Count;
				List<byte[]> dirs = new List<byte[]>(count); 
				List<MipData> mipCount = new List<MipData>(count);
				Encoding ascii = Encoding.ASCII;
				byte[] dirName = ascii.GetBytes(dirText);

				for (int i = 0; i < count; i++)
				{

					Layer item = (Layer)input.Layers[i];
					string val = item.Metadata.GetUserValue(fshMetadata);
					if (!string.IsNullOrEmpty(val))
					{
						string[] data = val.Split(',');

						dirs.Add(ascii.GetBytes(data[0]));

						mipCount.Add(new MipData(int.Parse(data[1], CultureInfo.InvariantCulture), bool.Parse(data[2]), item.Size)); 
					}
					else
					{
						dirs.Add(dirName);
						mipCount.Add(new MipData() { layerWidth = item.Width, layerHeight = item.Height });
					}
				}

				//write header
				output.Write(ascii.GetBytes("SHPI"), 0, 4); // write SHPI id
				output.Write(BitConverter.GetBytes(0), 0, 4); // placeholder for the length
				output.Write(BitConverter.GetBytes(count), 0, 4); // write the number of bitmaps in the list
				output.Write(ascii.GetBytes("G264"), 0, 4); // header directory

				int fshlen = 16 + (8 * count); // fsh length

				int bmpw, bmph; 
				for (int i = 0; i < count; i++)
				{
					output.Write(dirs[i], 0, 4);
					output.Write(BitConverter.GetBytes(fshlen), 0, 4);

					MipData data =  mipCount[i];
					for (int j = 0; j <= data.count; j++)
					{
						bmpw = (data.layerWidth >> j);
						bmph = (data.layerHeight >> j);

						fshlen += GetBmpDataSize(bmpw, bmph, format);               
					}
				}

				for (int i = 0; i < count; i++)
				{
					BitmapLayer bl = (BitmapLayer)input.Layers[i];
						
					// write the entry header
					int code = (int)format;
					long entryStart = output.Position;

					output.Write(BitConverter.GetBytes(code), 0, 4);
					output.Write(BitConverter.GetBytes((ushort)bl.Width), 0, 2);
					output.Write(BitConverter.GetBytes((ushort)bl.Height), 0, 2);
						
					MipData mips = mipCount[i];
					int nMips = mips.count; 

					ushort[] misc = new ushort[4];
					misc[0] = misc[1] = misc[2] = 0;
					misc[3] = (ushort)(nMips << 12);

					for (int j = 0; j < 4; j++)
					{
						output.Write(BitConverter.GetBytes(misc[j]), 0, 2);
					}

					Surface src =  bl.Surface;
					int width = bl.Width;
					int height = bl.Height;
					int dataLen = 0;
					Surface surf = src.Clone();

					for (int j = 0; j <= nMips; j++)
					{
						bmpw = (width >> j);
						bmph = (height >> j);


						if (j > 0) // resize the mipmaps
						{
							surf.Dispose();

							surf = new Surface(bmpw, bmph);
							surf.FitSurface(ResamplingAlgorithm.Bilinear, src);
						}

						dataLen = GetBmpDataSize(bmpw, bmph, format);

						byte[] data = SaveImageData(surf, format, dataLen);

						output.Write(data, 0, dataLen);
					}

					if (mips.count > 0)
					{
						long sectionLength = output.Position - entryStart;
						int newCode = (((int)sectionLength << 8) | code);

						output.Seek(entryStart, SeekOrigin.Begin);
						output.Write(BitConverter.GetBytes(newCode), 0, 4);	 
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

			if (format == FshFileFormat.TwentyFourBit)
			{
				data = new byte[dataLength + 2000];

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
				data = new byte[dataLength + 2000];

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
			else
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

			return data;
		}


	}
}
