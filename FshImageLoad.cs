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
			this.disposed = false;
			this.bitmaps = new List<FshLoadBitmapItem>();
			this.header = null;
		}

		public FshImageLoad(Stream input) : this()
		{
			this.Load(input);
		}

		private List<FshLoadBitmapItem> bitmaps;
		private FSHHeader header;

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

		private static MemoryStream UnpackQFS(Stream input)
		{
			input.Position = 0L;
			byte[] bytes = QfsComp.Decompress(input);

			return new MemoryStream(bytes);
		}

		public unsafe void Load(Stream input)
		{
			if (input.Length <= 4)
			{
				throw new FileFormatException(Resources.InvalidFshFile);
			}

			MemoryStream decompressedData = null;
			byte[] compSig = new byte[2];
			input.ProperRead(compSig, 0, 2);

			if ((compSig[0] & 0xfe) == 0x10 && compSig[1] == 0xfb)
			{
				decompressedData = UnpackQFS(input);
			}
			else
			{
				input.Position = 4L;

				input.ProperRead(compSig, 0, 2);

				if ((compSig[0] & 0xfe) == 0x10 && compSig[1] == 0xfb)
				{
					decompressedData = UnpackQFS(input);
				}
				else
				{
					input.Position = 0L;
				}
			}


			Stream stream = decompressedData ?? input;
			try
			{
				this.header = new FSHHeader(stream);

				int nBmps = header.ImageCount;

				FSHDirEntry[] dirs = new FSHDirEntry[nBmps];
				for (int i = 0; i < nBmps; i++)
				{
					dirs[i] = new FSHDirEntry(stream);
				}

				for (int i = 0; i < nBmps; i++)
				{
					stream.Seek((long)dirs[i].offset, SeekOrigin.Begin);
					int code = (stream.ReadInt32() & 0x7f);

					if (code == 0x7b)
					{
						throw new FileFormatException(Resources.IndexedFormatNotSupported);
					}
				}
				int size = header.Size;


				FSHEntryHeader[] entries = new FSHEntryHeader[nBmps];
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

					stream.Seek((long)dir.offset, SeekOrigin.Begin);
					FSHEntryHeader entry = new FSHEntryHeader(stream);
					entries[i] = entry;
 

					int code = (entry.code & 0x7f);
					bool entryCompressed = (entry.code & 0x80) > 0;
					bool isbmp = ((code == 0x60) || (code == 0x61) || (code == 0x7d) || (code == 0x7f) || (code == 0x7e) || (code == 0x78) || (code == 0x6d));

					if (isbmp)
					{
						FSHEntryHeader auxHeader = entry;
						int nAttach = 0;
						int auxOffset = dir.offset;
						while ((auxHeader.code >> 8) > 0)
						{
							auxOffset += (auxHeader.code >> 8);

							if ((auxOffset + 4) >= size)
							{
								break;
							}
							nAttach++;

							stream.Seek((long)auxOffset, SeekOrigin.Begin);
							auxHeader.code = stream.ReadInt32();
						}

						int numScales = (entry.misc[3] >> 12) & 0x0f;
						bool packedMbp = false;

						if (((entry.width % 1) << numScales) > 0 || ((entry.height % 1) << numScales) > 0)
						{
							numScales = 0;
						}

						if (numScales > 0 && !entryCompressed)
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
								default:
									bpp = 4;
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
								int length = ((bmpw * bmph) * bpp) / 2;
								mbpLen += length;
								mbpPadLen += length;
								
                                // DXT1 mipmaps smaller than 4x4 are also padded
                                int padding = ((16 - mbpLen) & 15);
								if (padding > 0)
								{
									mbpLen += padding;
									if (n == numScales)
									{
										mbpPadLen += padding;
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


						int width = (int)entry.width;
						int height = (int)entry.height;
						long bmpStartOffset = (long)(dir.offset + FSHEntryHeader.SizeOf);


						stream.Seek(bmpStartOffset, SeekOrigin.Begin);
						int dataSize = GetBmpDataSize(width, height, code);
						byte[] data = null;

						if (entryCompressed)
						{
							int compressedSize;

							if ((entry.code >> 8) > 0)
							{
								compressedSize = entry.code >> 8;
							}
							else
							{
								compressedSize = nextOffset - (int)bmpStartOffset;
							}

							byte[] comp = new byte[compressedSize];
							stream.ProperRead(comp, 0, compressedSize);

							data = QfsComp.Decompress(comp);
						}
						else
						{
							data = new byte[dataSize];
							stream.ProperRead(data, 0, dataSize);
						}

						FshLoadBitmapItem item = new FshLoadBitmapItem(width, height);
						Surface dest = item.Surface;

						if (code == 0x60 || code == 0x61) // DXT1 or DXT3
						{
							byte[] rgba = DXTComp.UnpackDXTImage(data, width, height, (code == 0x60));

							fixed (byte* ptr = rgba)
							{
								int srcStride = width * 4;
								for (int y = 0; y < height; y++)
								{
									byte* src = ptr + (y * srcStride);
									ColorBgra* p = dest.GetRowAddressUnchecked(y);
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
						}
						else if (code == 0x7d) // 32-bit RGBA (BGRA pixel order)
						{
							fixed (byte* ptr = data)
							{
								for (int y = 0; y < height; y++)
								{
									uint* src = (uint*)ptr + (y * width);
									ColorBgra* p = dest.GetRowAddressUnchecked(y);
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
							new UnaryPixelOps.SetAlphaChannelTo255().Apply(dest, dest.Bounds);

							fixed (byte* ptr = data)
							{
								int stride = width * 3;
								for (int y = 0; y < height; y++)
								{
									byte* src = ptr + (y * stride);
									ColorBgra* p = dest.GetRowAddressUnchecked(y);
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
						else if (code == 0x7e) // 16-bit (1:5:5:5)
						{

							fixed (byte* ptr = data)
							{
								ushort* sPtr = (ushort*)ptr;
								for (int y = 0; y < height; y++)
								{
									ushort* src = sPtr + (y * width);
									ColorBgra* p = dest.GetRowAddressUnchecked(y);

									for (int x = 0; x < width; x++)
									{
										p->B = (byte)((src[0] & 0x1f) << 3);
										p->G = (byte)(((src[0] >> 5) & 0x3f) << 3);
										p->R = (byte)(((src[0] >> 10) & 0x1f) << 3);
										if ((src[0] & 0x8000) > 0)
										{
											p->A = 255;
										}


										p++;
										src++;
									}
								}
							}

						}
						else if (code == 0x78) // 16-bit (0:5:6:5)
						{
							new UnaryPixelOps.SetAlphaChannelTo255().Apply(dest, dest.Bounds);

							fixed (byte* ptr = data)
							{
								ushort* sPtr = (ushort*)ptr;
								for (int y = 0; y < height; y++)
								{
									ushort* src = sPtr + (y * width);
									ColorBgra* p = dest.GetRowAddressUnchecked(y);

									for (int x = 0; x < width; x++)
									{
										p->B = (byte)((src[0] & 0x1f) << 3);
										p->G = (byte)(((src[0] >> 5) & 0x3f) << 2);
										p->R = (byte)(((src[0] >> 11) & 0x1f) << 3);

										p++;
										src++;
									}
								}
							}


						}
						else if (code == 0x6d) // 16-bit (4:4:4:4)
						{
							fixed (byte* ptr = data)
							{
								int stride = width * 2;
								for (int y = 0; y < height; y++)
								{
									byte* src = ptr + (y * stride);
									ColorBgra* p = dest.GetRowAddressUnchecked(y);

									for (int x = 0; x < width; x++)
									{
										p->B = (byte)((src[0] & 15) * 0x11);
										p->G = (byte)((src[0] >> 4) * 0x11);
										p->R = (byte)((src[1] & 15) * 0x11);
										p->A = (byte)((src[1] >> 4) * 0x11);

										src += 2;
										p++;
									}
								}
							}

						}

						List<FSHAttachment> attach = null;

						if (nAttach > 0)
						{
							attach = new List<FSHAttachment>(nAttach);
							auxOffset = dir.offset;
							auxHeader = entry;
							for (int j = 0; j < nAttach; j++)
							{
								auxOffset += (auxHeader.code >> 8);

								if ((auxOffset + 4) >= size)
								{
									break;
								}
								stream.Seek((long)auxOffset, SeekOrigin.Begin);

								auxHeader = new FSHEntryHeader()
								{
									code = stream.ReadInt32()
								};
								int attachCode = (auxHeader.code & 0xff);

								if (attachCode == 0x22 || attachCode == 0x24 || attachCode == 0x29 || attachCode == 0x2a || attachCode == 0x2d)
								{
									continue; // Skip any Indexed color palettes.
								}

								if (attachCode == 0x6f || attachCode == 0x69 || attachCode == 0x7c)
								{
									try
									{
										auxHeader.width = stream.ReadUInt16();
										auxHeader.height = stream.ReadUInt16();
										if (attachCode == 0x69 || attachCode == 0x7c)
										{
											for (int m = 0; m < 4; m++)
											{
												auxHeader.misc[m] = stream.ReadUInt16();
											}
										}
									}
									catch (EndOfStreamException)
									{
										break;
									}
								}

								byte[] attachBytes = null;
								int len = 0;
								bool binaryData = false;
								switch (attachCode)
								{
									case 0x6f: // TXT                                    
									case 0x69: // ETXT full header
										attachBytes = new byte[auxHeader.width];
										stream.ProperRead(attachBytes, 0, attachBytes.Length);
										break;
									case 0x70: // ETXT 16 bytes
										attachBytes = new byte[12];
										stream.ProperRead(attachBytes, 0, attachBytes.Length);
										break;
									case 0x7c: // Pixel region, does not have any data other than the misc fields of the header.
										attachBytes = new byte[0];
										break;
									default: // Binary data
										len = (auxHeader.code >> 8);
										if (len == 0)
										{
											len = nextOffset - auxOffset;
										}
										if (len > 16384)
										{
											// attachment data too large skip it
											continue;
										}
										stream.Seek(auxOffset, SeekOrigin.Begin);
										attachBytes = new byte[len];
										stream.ProperRead(attachBytes, 0, len);
										binaryData = true;

										break;
								}

#if DEBUG
								System.Diagnostics.Debug.Assert(data != null);
#endif

								attach.Add(new FSHAttachment(auxHeader, attachBytes, binaryData));
							}

						}


						item.MetaData = new FshMetadata(dir.name, numScales, packedMbp, entry.misc, entryCompressed, attach);

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
				if (decompressedData != null)
				{
					decompressedData.Dispose();
					decompressedData = null;
				}
			}
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
