using System;
using System.Diagnostics;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.IO;
using PaintDotNet;
using PaintDotNet.Data;
using System.Windows.Forms;
using FSHLib;

namespace FSHfiletype
{

    internal class QfsFileType
        : PaintDotNet.FileType
    {


        public QfsFileType()
            : base("Compressed Fsh", FileTypeFlags.SupportsLoading | FileTypeFlags.SupportsSaving, new string[] { ".qfs" })
        {

        }
        protected override SaveConfigToken OnCreateDefaultSaveConfigToken()
        {
            return new FshSaveConfigToken(false, false, true, 2, "FiSH", false, true, true);
        }
        public override SaveConfigWidget CreateSaveConfigWidget()
        {
            bool origalpha = false;
            if (loadimage != null && loadimage.Bitmaps.Count > 0)
            {
                origalpha = true;
            }
            QfsSaveConfigDialog dialog = new QfsSaveConfigDialog();
            dialog.origAlpha.Enabled = origalpha;
            if (!origalpha)
            {
                dialog.imgtransalphaRadio.Checked = true;
            }
            dialog.savebmptype = bmptype;
            return dialog;
        }
        private string bmptype = string.Empty;
        private FSHImage loadimage = null;
        private BlendBitmap blbmp = new BlendBitmap();
        private byte[] headdir = null; 

        private void Reset24bitAlpha(BitmapItem item)
        {
            if (item.BmpType == FSHBmpType.TwentyFourBit)
            {
                Bitmap alpha = new Bitmap(item.Bitmap.Width, item.Bitmap.Height);
                for (int y = 0; y < alpha.Height; y++)
                {
                    for (int x = 0; x < alpha.Width; x++)
                    {
                        alpha.SetPixel(x, y, Color.White);
                    }
                }
                item.Alpha = alpha;
            }
        }
        protected override Document OnLoad(System.IO.Stream input)
        {
            try
            {
                loadimage = new FSHImage(input);
                bool combinealpha = true;
                headdir = loadimage.Header.dirID;
                if (Encoding.ASCII.GetString(loadimage.Header.dirID) == "G315")
                {
                    combinealpha = false;
                }
                BitmapItem bmpitem = new BitmapItem();
                bmpitem = (BitmapItem)loadimage.Bitmaps[0];

                Document doc = new Document(bmpitem.Bitmap.Width, bmpitem.Bitmap.Height);
                for (int l = 0; l < loadimage.Bitmaps.Count; l++)
                {
                    bmpitem = (BitmapItem)loadimage.Bitmaps[l];
                    Reset24bitAlpha(bmpitem);
                    int w = bmpitem.Bitmap.Width;
                    int h = bmpitem.Bitmap.Height;
                    bmptype = bmpitem.BmpType.ToString();

                    buildlayers(doc, l, w, h, combinealpha);
                }

                return doc;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error loading Fsh");
                return null;
            }
        }
        private void buildlayers(Document doc, int layercnt, int fshwidth, int fshheight, bool combinealpha)
        {
            BitmapLayer bl = null;

            if (layercnt == 0)
            {
                bl = new BitmapLayer(fshwidth, fshheight);
                bl.IsBackground = true;
                bl.Name = "Fsh Bitmap" + layercnt.ToString();
            }
            else
            {
                bl = new BitmapLayer(fshwidth, fshheight);
                bl.Name = "Fsh Bitmap" + layercnt.ToString();
            }

            Surface bgsur = bl.Surface;
            BitmapItem tempitem = (BitmapItem)loadimage.Bitmaps[layercnt];
            BitmapLayer al = null;
            if (combinealpha)
            {
                Bitmap combimg = (Bitmap)blbmp.BlendBmp(tempitem);
                bgsur = Surface.CopyFromBitmap(combimg);
            }
            else
            {

                bgsur = Surface.CopyFromBitmap(tempitem.Bitmap);

                Surface alpha = Surface.CopyFromBitmap(tempitem.Alpha);
                al = new BitmapLayer(alpha);
                al.Name = "Fsh Alpha" + layercnt.ToString();

            }

            bl.Surface.CopySurface(bgsur);
            doc.Layers.Add(bl);
            if (al != null)
            {
                doc.Layers.Add(al);
            }
        }
        private bool thabort()
        {
            return false;
        }
       
        private int ParseLayerName(string name)
        {
            string n;

            if (name.Length > 11)
            {
                n = name.Substring(10, (name.Length - 10));
            }
            else
            {
                n = name.Substring(10);
            }
            return int.Parse(n);
        }
        private int ParseAlphaName(string name)
        {
            int len = "Fsh Alpha".Length;
            string temp = name.Substring(len, (name.Length - len));
            return int.Parse(temp);
        }
        private bool useFshwriteComp = true;
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
                            if (headdir != null)
                            {
                                fw.HeadDir = headdir;
                            }
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
        protected override void OnSave(Document input, Stream output, SaveConfigToken token, Surface scratchSurface, ProgressEventHandler callback)
        {
            FshSaveConfigToken fshtoken = (FshSaveConfigToken)token;


            try
            {
                FSHImage saveimg = new FSHImage();

                BitmapItem bmpitem = new BitmapItem();
                FSHImage[] mipimgs = null;

                this.useFshwriteComp = fshtoken.FshwriteComp;

                using (RenderArgs ra = new RenderArgs(scratchSurface))
                {
                    Bitmap srcbmp = ra.Surface.CreateAliasedBitmap();
                    saveimg.IsCompressed = true;

                    if (input.Width >= 128 && input.Height >= 128)
                    {
                        fshtoken.GenmipEnabled = true;
                    }
                    else
                    {
                        fshtoken.GenmipEnabled = false;
                    }

                    for (int l = 0; l < input.Layers.Count; l++)
                    {
                        BitmapLayer bl = (BitmapLayer)input.Layers[l];
                        if (!bl.Name.Contains("Alpha"))
                        {
                            bl.Render(ra, bl.Bounds);
                            Bitmap bitmap = scratchSurface.CreateAliasedBitmap();

                            bmpitem.Bitmap = bitmap;

                            if (fshtoken.OrigAlpha)
                            {
                                if (loadimage != null && loadimage.Bitmaps.Count > 0)
                                {
                                    BitmapItem temp = (BitmapItem)loadimage.Bitmaps[ParseLayerName(bl.Name)];
                                    bmpitem.Alpha = new Bitmap(temp.Alpha);
                                }
                            }
                            else if (fshtoken.Genmap)
                            {
                                Surface alphasrc = new Surface(input.Size);
                                alphasrc.Clear(ColorBgra.White);
                                using (RenderArgs alphagen = new RenderArgs(alphasrc))
                                {
                                    bmpitem.Alpha = new Bitmap(alphagen.Bitmap);
                                    bmpitem.BmpType = FSHBmpType.DXT1;
                                    fshtoken.Fshtype = 2;
                                }
                            }
                            else if (fshtoken.Alphatrans)
                            {
                                Bitmap testbmp = new Bitmap(bmpitem.Bitmap.Width, bmpitem.Bitmap.Height, PixelFormat.Format32bppArgb);

                                for (int y = 0; y < testbmp.Height; y++)
                                {
                                    for (int x = 0; x < testbmp.Width; x++)
                                    {
                                        Color srcpxl = srcbmp.GetPixel(x, y);
                                        Color destpxl = testbmp.GetPixel(x, y);
                                        testbmp.SetPixel(x, y, Color.FromArgb(srcpxl.A, srcpxl.A, srcpxl.A, srcpxl.A));
                                        while (testbmp.GetPixel(x, y).A < 255)
                                        {
                                            testbmp.SetPixel(x, y, Color.FromArgb(srcpxl.A, srcpxl.A, srcpxl.A));
                                        }
                                    }
                                }
                                bmpitem.Alpha = testbmp;
                                bmpitem.BmpType = FSHBmpType.DXT3;
                                fshtoken.Fshtype = 3;
                            }
                            else
                            {
                                Debug.WriteLine("No alpha map file loaded");
                                Surface alphasrc = new Surface(input.Size);
                                alphasrc.Clear(ColorBgra.White);
                                using (RenderArgs alphagen = new RenderArgs(alphasrc))
                                {
                                    bmpitem.Alpha = new Bitmap(alphagen.Bitmap);
                                    bmpitem.BmpType = FSHBmpType.DXT1;
                                    fshtoken.Fshtype = 2;
                                }
                            }

                            switch (fshtoken.Fshtype)
                            {
                                case 0:
                                    bmpitem.BmpType = FSHBmpType.TwentyFourBit;
                                    break;

                                case 1:
                                    bmpitem.BmpType = FSHBmpType.ThirtyTwoBit;
                                    break;

                                case 2:
                                    bmpitem.BmpType = FSHBmpType.DXT1;
                                    break;

                                case 3:
                                    bmpitem.BmpType = FSHBmpType.DXT3;
                                    break;

                                default:
                                    bmpitem.BmpType = FSHBmpType.DXT3;
                                    break;
                            }
                            saveimg.Bitmaps.Add(bmpitem);
                            saveimg.UpdateDirty();


                            using (MemoryStream ms = new MemoryStream())
                            {
                                SaveFsh(ms, saveimg);
                                saveimg = new FSHImage(ms); // save and reload the updated image 
                            }

                            if (fshtoken.GenmipEnabled && fshtoken.Genmip)
                            {
                                if (bmpitem.Bitmap.Width >= 128 && bmpitem.Bitmap.Height >= 128)
                                {

                                    Bitmap[] bmps = new Bitmap[4];
                                    Bitmap[] alphas = new Bitmap[4];

                                    // 0 = 8, 1 = 16, 2 = 32, 3 = 64
                                    Image.GetThumbnailImageAbort abort = new Image.GetThumbnailImageAbort(thabort);
                                    if (bmpitem.Bitmap.Width >= 128 && bmpitem.Bitmap.Height >= 128)
                                    {
                                        Bitmap bmp = new Bitmap(bmpitem.Bitmap);
                                        bmps[0] = (Bitmap)bmp.GetThumbnailImage(8, 8, abort, IntPtr.Zero);
                                        bmps[1] = (Bitmap)bmp.GetThumbnailImage(16, 16, abort, IntPtr.Zero);
                                        bmps[2] = (Bitmap)bmp.GetThumbnailImage(32, 32, abort, IntPtr.Zero);
                                        bmps[3] = (Bitmap)bmp.GetThumbnailImage(64, 64, abort, IntPtr.Zero);
                                        //alpha
                                        Bitmap alpha = new Bitmap(bmpitem.Alpha);
                                        alphas[0] = (Bitmap)alpha.GetThumbnailImage(8, 8, abort, IntPtr.Zero);
                                        alphas[1] = (Bitmap)alpha.GetThumbnailImage(16, 16, abort, IntPtr.Zero);
                                        alphas[2] = (Bitmap)alpha.GetThumbnailImage(32, 32, abort, IntPtr.Zero);
                                        alphas[3] = (Bitmap)alpha.GetThumbnailImage(64, 64, abort, IntPtr.Zero);

                                        if (mipimgs == null)
                                        {
                                            mipimgs = new FSHImage[4];
                                        }
                                        for (int i = 3; i >= 0; i--)
                                        {
                                            if (bmps[i] != null && alphas[i] != null)
                                            {
                                                Rectangle bmprect = new Rectangle(0, 0, bmps[i].Width, bmps[i].Height);
                                                BitmapItem mipitm = new BitmapItem();
                                                mipitm.Bitmap = bmps[i].Clone(bmprect, PixelFormat.Format32bppArgb);
                                                mipitm.Alpha = alphas[i].Clone(bmprect, PixelFormat.Format24bppRgb);

                                                if (bmpitem.BmpType == FSHBmpType.DXT3 || bmpitem.BmpType == FSHBmpType.ThirtyTwoBit)
                                                {
                                                    mipitm.BmpType = FSHBmpType.DXT3;
                                                }

                                                if (mipimgs[i] == null)
                                                {
                                                    mipimgs[i] = new FSHImage();
                                                }

                                                mipimgs[i].Bitmaps.Add(mipitm);
                                                mipimgs[i].UpdateDirty();

                                            }
                                        }
                                    }

                                }
                            }

                        }
                        else
                        {
                            if (!fshtoken.OrigAlpha)
                            {
                                int idx = ParseAlphaName(bl.Name);
                                BitmapItem item = (BitmapItem)saveimg.Bitmaps[idx];

                                bl.Render(ra, bl.Bounds);

                                Bitmap alpha = scratchSurface.CreateAliasedBitmap();
                                item.Alpha = alpha;
                                saveimg.Bitmaps.Insert(idx, item);
                                saveimg.UpdateDirty();
                                if (fshtoken.GenmipEnabled && fshtoken.Genmip && mipimgs != null)
                                {
                                    Bitmap[] alphas = new Bitmap[4];
                                    Image.GetThumbnailImageAbort abort = new Image.GetThumbnailImageAbort(thabort);

                                    alphas[0] = (Bitmap)alpha.GetThumbnailImage(8, 8, abort, IntPtr.Zero);
                                    alphas[1] = (Bitmap)alpha.GetThumbnailImage(16, 16, abort, IntPtr.Zero);
                                    alphas[2] = (Bitmap)alpha.GetThumbnailImage(32, 32, abort, IntPtr.Zero);
                                    alphas[3] = (Bitmap)alpha.GetThumbnailImage(64, 64, abort, IntPtr.Zero);
                                    for (int i = 3; i >= 0; i--)
                                    {
                                        BitmapItem bi = (BitmapItem)mipimgs[i].Bitmaps[idx];
                                        bi.Alpha = alphas[i];
                                        if (alphas[i].GetPixel(0, 0).ToArgb() == Color.Black.ToArgb())
                                        {
                                            bi.BmpType = FSHBmpType.DXT3;
                                        }
                                        else
                                        {
                                            bi.BmpType = FSHBmpType.DXT1;
                                        }
                                        mipimgs[i].Bitmaps.Insert(idx, bi);
                                        mipimgs[i].UpdateDirty();
                                    }
                                }

                            }

                        }
                    }
                    
                }

                SaveFsh(output, saveimg);

                if (fshtoken.GenmipEnabled && fshtoken.Genmip)
                {

                    string filename = string.Empty;
                    FileStream fs = output as FileStream;
                    if (fs != null)
                    {
                        filename = fs.Name;
                    }

                    if (!string.IsNullOrEmpty(filename))
                    {
                        string dir = Path.GetDirectoryName(filename);
                        string tempdir = Path.GetDirectoryName(Path.GetTempPath());
                        if (string.CompareOrdinal(dir, tempdir) != 0)
                        {
                            string filepath = null;
                            for (int i = 3; i >= 0; i--)
                            {
                                if (mipimgs[i] != null)
                                {

                                    filepath = GetFileName(filename, "_s" + i.ToString());
                                    using (FileStream fstream = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.Write))
                                    {
                                        mipimgs[i].IsCompressed = true;
                                        SaveFsh(fstream, mipimgs[i]);
                                    }

                                }

                            }
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Saving Fsh", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private string GetFileName(string path, string add)
        {
            return Path.Combine(Path.GetDirectoryName(path) + Path.DirectorySeparatorChar, Path.GetFileNameWithoutExtension(path) + add + Path.GetExtension(path));
        }
        
    }
}
