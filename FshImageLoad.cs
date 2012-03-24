using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using FSHfiletype.Properties;
using PaintDotNet.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using PaintDotNet;

namespace FSHfiletype
{
	internal sealed class FshImageLoad : IDisposable
	{
		public FshImageLoad()
		{
			disposed = false;
			bitmaps = new List<FshLoadBitmapItem>();
			header = new FSHHeader();
			dirs = null;
			entries = null;
		}

		public FshImageLoad(Stream input) : this()
		{
			this.Load(input);
		}

		private List<FshLoadBitmapItem> bitmaps;
		private FSHHeader header;
		private FSHDirEntry[] dirs;
		private FSHEntryHeader[] entries;

		public List<FshLoadBitmapItem> Bitmaps
		{
			get
			{
				return bitmaps;
			}
		}

		public FSHHeader Header
		{
			get
			{
				return header;
			}
		}

		private static int GetBmpDataSize(int width, int height, int code)
		{
			int size = 0;
			switch (code)
			{
				case 0x6d:
				case 0x78:
				case 0x7e:
					size = (width * height) * 2;
					break;
				case 0x7d:
					size = (width * height) * 4;
					break;
				case 0x7f:
					size = (width * height) * 3;
					break;
				case 0x60:
					size = (width * height) / 2;
					break;
				case 0x61:
					size = (width * height);
					break;
			}

			return size;
		}

		private MemoryStream Decomp(Stream input)
		{
			byte[] bytes = QfsComp.Decomp(input);

			return new MemoryStream(bytes);
		}

		public unsafe void Load(Stream input)
		{
			if (input.Length <= 4)
			{
				throw new FileFormatException(Resources.InvalidFshFile);
			}
			
			MemoryStream ms = null;
			byte[] compSig = new byte[2];

			if (compSig[0] == 16 && compSig[1] == 0xfb)
			{
				ms = this.Decomp(input);
			}
			else
			{
				input.Position = 4L;

				input.Read(compSig, 0, 2);

				if (compSig[0] == 16 && compSig[1] == 0xfb)
				{
					ms = this.Decomp(input);
				}
				else
				{
					input.Position = 0L;
					byte[] bytes = new byte[input.Length];
					input.ProperRead(bytes, 0, bytes.Length);

					ms = new MemoryStream(bytes);
				}
			}
		   
			try 
			{	
				header = new FSHHeader(){ SHPI = new byte[4], dirID = new byte[4] };
				ms.Read(header.SHPI, 0, 4);

				if (Encoding.ASCII.GetString(header.SHPI) != "SHPI")
				{
					throw new FileFormatException(Resources.InvalidFshHeader);
				}

				header.size = ms.ReadInt32();
				header.numBmps = ms.ReadInt32();
				ms.Read(header.dirID, 0, 4);

				int nBmps = header.numBmps;

				this.dirs = new FSHDirEntry[nBmps];
				for (int i = 0; i < nBmps; i++)
				{
					dirs[i] = new FSHDirEntry() { name = new byte[4] };
					ms.Read(dirs[i].name, 0, 4);
					dirs[i].offset = ms.ReadInt32();
				}

				for (int i = 0; i < nBmps; i++)
				{
					ms.Seek((long)dirs[i].offset, SeekOrigin.Begin);
					int code = (ms.ReadInt32() & 0x7f);

					if (code == 0x7b)
					{
						throw new FileFormatException(Resources.UnsupportedFshFormat);
					}
				}
				int size = header.size;


				this.entries = new FSHEntryHeader[nBmps];
				this.bitmaps = new List<FshLoadBitmapItem>(nBmps);

				int nextOffset = size;
				for (int i = 0; i < nBmps; i++)
				{ 
					FSHDirEntry dir = dirs[i];
					for (int j = 0; j < nBmps; j++)
					{
						if ((dirs[j].offset < nextOffset) && (dirs[j].offset > nextOffset))
						{
							nextOffset = dirs[j].offset;
						}
					}

					ms.Seek((long)dir.offset, SeekOrigin.Begin);
					FSHEntryHeader entry = entries[i];
					entry = new FSHEntryHeader() { misc = new ushort[4] };
					entry.code = ms.ReadInt32();
					entry.width = ms.ReadUInt16();
					entry.height = ms.ReadUInt16();
					for (int m = 0; m < 4; m++)
					{
						entry.misc[m] = ms.ReadUInt16();
					}

					int code = (entry.code & 0x7f);
 
					bool isbmp = ((code == 0x60) || (code == 0x61) || (code == 0x7d) || (code == 0x7f));
					
					int numScales = (entry.misc[3] >> 12) & 0x0f;
					bool packedMbp = false;

					if (isbmp)
					{
						FSHEntryHeader aux = entry;
						int nAttach = 0;
						int auxofs = dir.offset;
						while ((aux.code >> 8) > 0)
						{
							auxofs += (aux.code >> 8);

							if ((auxofs + 4) >= size)
							{
								break;
							}
							nAttach++;
						}

						if (((entry.width % 1) << numScales) > 0 || ((entry.height % 1) << numScales) > 0)
						{
							numScales = 0;
						}

						if (numScales > 0)
						{
							int bpp = 0;
							int mbpLen = 0;
							int mbpPadLen = 0;
							int bmpw = 0;
							switch (code)
							{
								case 0x7b:
								case 0x61:
									bpp = 2;
									break;
								case 0x7d:
									bpp = 8;
									break;
								case 0x7f:
									bpp = 6;
									break;
								case 0x60:
									bpp = 1;
									break;
							}
							for (int n = 0; n <= numScales; n++)
							{
								bmpw = (entry.width >> n);
								int bmph = (entry.height >> n);
								if (code == 0x60)
								{
									bmpw += (4 - bmpw) & 3;
									bmph += (4 - bmph) & 3;
								}
								mbpLen += (bmpw * bmph) * bpp / 2;
								mbpPadLen += (bmpw * bmph) * bpp / 2;

								if (code != 0x60)
								{
									mbpLen += ((16 - mbpLen) & 15); // padding
									if (n == numScales)
									{
										mbpPadLen += ((16 - mbpPadLen) & 15);
									}
								}
							}
							if (((entry.code >> 8) != mbpLen + 16) && ((entry.code >> 8) != 0) ||
								((entry.code >> 8) == 0) && ((mbpLen + dir.offset + 16) != size))
							{
								packedMbp = true;
								if (((entry.code >> 8) != mbpPadLen + 16) && ((entry.code >> 8) != 0) ||
								((entry.code >> 8) == 0) && ((mbpPadLen + dir.offset + 16) != size))
								{
									numScales = 0;
								}
							}
						}


						FshLoadBitmapItem item = null;
						int width = (int)entry.width;
						int height = (int)entry.height;
						long bmppos = (long)(dir.offset + 16);


						ms.Seek(bmppos, SeekOrigin.Begin);
                        int dataSize = GetBmpDataSize(width, height, code);
						byte[] data = null;

                        bool compressed = false;
						if ((entry.code & 0x80) > 0)
						{
                            compressed = true;
                            int compSize = nextOffset - (int)bmppos;
                            byte[] comp = new byte[compSize];
                            ms.ProperRead(comp, 0, compSize);

                            data = QfsComp.Decomp(comp);
                            
						}
						else
						{
                            data = new byte[dataSize];
                            ms.ProperRead(data, 0, dataSize);
						}

						if (code == 0x60 || code == 0x61) // DXT1 or DXT3
						{
							byte[] rgba = DXTComp.UnpackDXTImage(data, width, height, (code == 0x60));
							item = BuildDxtBitmap(rgba, width, height);
						}
						else if (code == 0x7d) // 32-bit RGBA (BGRA pixel order)
						{
							item = new FshLoadBitmapItem(width, height);

							fixed (byte* ptr = data)
							{
								Surface surf = item.Surface;
								for (int y = 0; y < height; y++)
								{
									uint* src = (uint*)ptr + (y * width);
									ColorBgra* p = surf.GetRowAddressUnchecked(y);
									for (int x = 0; x < width; x++)
									{
										p->Bgra = *src; // since it is BGRA just read it as a UInt32

										p++;
										src++;
									}
								}
							}

						}
						else if (code == 0x7f) // 24-bit RGB (BGR pixel order)
						{
							item = new FshLoadBitmapItem(width, height);

							fixed (byte* ptr = data)
							{
								Surface surf = item.Surface;
								new UnaryPixelOps.SetAlphaChannelTo255().Apply(surf, surf.Bounds);

								int stride = width * 3;
								for (int y = 0; y < height; y++)
								{
									byte* src = ptr + (y * stride);
									ColorBgra* p = surf.GetRowAddressUnchecked(y);
									for (int x = 0; x < width; x++)
									{
										p->B = src[0]; // blue
										p->G = src[1]; // green
										p->R = src[2]; // red 

										p++;
										src += 3;
									}
								}
							}

						}

                        item.MetaData = new FshMetadata(dir.name, numScales, packedMbp, entry.misc, compressed);

						this.bitmaps.Add(item);
					}

				}
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				if (ms != null)
				{
					ms.Dispose();
					ms = null;
				}
			}
		}

		/// <summary>
		/// Build the alpha and color bitmaps from the uncompressed DXT image data.
		/// </summary>
		/// <param name="data">The image data.</param>
		/// <param name="bmp">The output color bitmap.</param>
		/// <param name="alpha">The output alpha bitmap.</param>
		private unsafe FshLoadBitmapItem BuildDxtBitmap(byte[] data, int width, int height)
		{
			FshLoadBitmapItem item = new FshLoadBitmapItem(width, height);
			Surface surf = item.Surface; 

			fixed (byte* ptr = data)
			{
				int srcStride = width * 4;
				for (int y = 0; y < height; y++)
				{
					byte* src = ptr + (y * srcStride); 
					ColorBgra* p = surf.GetRowAddressUnchecked(y);
					for (int x = 0; x < width; x++)
					{
						p->R = src[0]; // red 
						p->G = src[1]; // green
						p->B = src[2]; // blue
						p->A = src[3]; // alpha

						src += 4;
						p++;
					}
				} 
			}
			   
			
		   

			return item;
		}


		private bool disposed;
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		private void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					foreach (var item in bitmaps)
					{
						item.Dispose();
					}
					disposed = true;
				}
			}
		}
	}
}
