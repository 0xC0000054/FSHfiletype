using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PaintDotNet;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using FSHLib;
using System.Windows.Forms;
using FSHfiletype.Properties;

namespace FSHfiletype
{
    class FshFile
    {
        public FshFile()
        {
            useFshwriteComp = false;
        }
        public FshFile(Stream input) : this()
        {
            this.Load(input);
        }

        public Document Load(Stream input)
        {
            try
            {
                using (FshImageLoad loadimage = new FshImageLoad(input))
                {
                    Document doc = new Document(loadimage.Bitmaps[0].Surface.Width, loadimage.Bitmaps[0].Surface.Height);
                    for (int l = 0; l < loadimage.Bitmaps.Count; l++)
                    {
                        FshLoadBitmapItem bmpitem = loadimage.Bitmaps[l];
                        int width = bmpitem.Surface.Width;
                        int height = bmpitem.Surface.Height;

                        BitmapLayer bl = new BitmapLayer(width, height) { Name = Resources.FshLayerTitle + l.ToString() };

                        if (l == 0)
                        {
                            bl.IsBackground = true;
                        }

                        bl.Surface.CopySurface(bmpitem.Surface);
                        
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

        /// <summary>
        /// Saves a fsh using either FshWrite or FSHLib
        /// </summary>
        /// <param name="fs">The stream to save to</param>
        /// <param name="image">The image to save</param>
        private void SaveFsh(Stream fs, FSHImage image)
        {
            try
            {
                if (IsDXTFsh(image) && useFshwriteComp)
                {
                    Fshwrite fw = new Fshwrite();
                    foreach (BitmapItem bi in image.Bitmaps)
                    {
                        if ((bi.Bitmap != null && bi.Alpha != null) && bi.BmpType == FSHBmpType.DXT1 || bi.BmpType == FSHBmpType.DXT3)
                        {
                            fw.bmp.Add(bi.Bitmap);
                            fw.alpha.Add(bi.Alpha);
                            fw.dir.Add(bi.DirName);
                            fw.code.Add((int)bi.BmpType);
                        }
                    }
                    fw.WriteFsh(fs);
                }
                else
                {
                    image.Save(fs);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Test if the fsh only contains DXT1 or DXT3 items
        /// </summary>
        /// <param name="image">The image to test</param>
        /// <returns>True if successful otherwise false</returns>
        private bool IsDXTFsh(FSHImage image)
        {
            bool result = true;
            foreach (BitmapItem bi in image.Bitmaps)
            {
                if (bi.BmpType != FSHBmpType.DXT3 && bi.BmpType != FSHBmpType.DXT1)
                {
                    result = false;
                }
            }
            return result;
        }
        private bool useFshwriteComp;

        public void Save(Document input, Stream output, PropertyBasedSaveConfigToken token, Surface scratchSurface)
        {
            try
            {
                FSHImage saveimg = new FSHImage();

                BitmapItem bmpitem = new BitmapItem();
                Dictionary<int, Bitmap> alphalayerlist = new Dictionary<int, Bitmap>();

                this.useFshwriteComp = (bool)token.GetProperty(PropertyNames.FshWriteCompression).Value;
                bool alphaTrans = (bool)token.GetProperty(PropertyNames.AlphaFromTransparency).Value;
                FshFileFormat format = (FshFileFormat)token.GetProperty(PropertyNames.FileType).Value;

                using (RenderArgs ra = new RenderArgs(scratchSurface))
                {
                    for (int l = 0; l < input.Layers.Count; l++)
                    {
                        BitmapLayer bl = (BitmapLayer)input.Layers[l];
                        if (bl.Visible)
                        {
                            
                            bl.Render(ra, bl.Bounds);

                            bmpitem.Bitmap = scratchSurface.CreateAliasedBitmap().Clone(bl.Bounds, PixelFormat.Format24bppRgb);

                            if (alphaTrans && format != FshFileFormat.TwentyFourBit)
                            {
                                using(Bitmap testbmp = new Bitmap(bmpitem.Bitmap.Width, bmpitem.Bitmap.Height, PixelFormat.Format24bppRgb))
	                            {
                                    unsafe
                                    {
                                        BitmapData data = testbmp.LockBits(bl.Bounds, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                                        try
                                        {
                                            for (int y = 0; y < testbmp.Height; y++)
                                            {
                                                ColorBgra* src = scratchSurface.GetRowAddressUnchecked(y);
                                                byte* dst = (byte*)data.Scan0.ToPointer() + (y * data.Stride); 
                                                for (int x = 0; x < testbmp.Width; x++)
                                                {
                                                    dst[0] = dst[1] = dst[2] = src->A;

                                                    src++;
                                                    dst += 3;
                                                }
                                            }
                                        }
                                        finally
                                        {
                                            testbmp.UnlockBits(data);
                                        }
                                    }
                                    bmpitem.Alpha = testbmp.CloneT<Bitmap>(); 
	                            }

                                if (format == FshFileFormat.ThirtyTwoBit)
                                {
                                    bmpitem.BmpType = FSHBmpType.ThirtyTwoBit;
                                }
                                else
                                {
                                    bmpitem.BmpType = FSHBmpType.DXT3;
                                }


                            }
                            else
                            {
                                using (Surface alphasrc = new Surface(input.Size))
                                {
                                    alphasrc.Clear(ColorBgra.White);
                                    using (RenderArgs alphagen = new RenderArgs(alphasrc))
                                    {
                                        bmpitem.Alpha = alphagen.Bitmap.CloneT<Bitmap>();
                                        if (format == FshFileFormat.TwentyFourBit)
                                        {
                                            bmpitem.BmpType = FSHBmpType.TwentyFourBit;
                                        }
                                        else
                                        {
                                            bmpitem.BmpType = FSHBmpType.DXT1;
                                        }
                                    }
                                }
                            }

                            saveimg.Bitmaps.Add(bmpitem);
                        }
                    }

                    saveimg.UpdateDirty();
                    SaveFsh(output, saveimg);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Resources.ErrorSavingCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


    }
}
