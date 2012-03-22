using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PaintDotNet;

namespace FSHfiletype
{
    internal class FshLoadBitmapItem : IDisposable
    {
        private Surface surface;
        private string dirName;
        private int mipCount;
        private bool mipPadding;
        private ushort[] misc;

        public Surface Surface
        {
            get
            {
                return surface;
            }
            set
            {
                surface = value;
            }
        }

        public string DirName
        {
            get
            {
                return dirName;
            }
            set
            {
                dirName = value;
            }
        }

        public int EmbeddedMipCount
        {
            get
            {
                return mipCount;
            }
            set
            {
                mipCount = value;
            }
        }

        public bool MipPadding
        {
            get
            {
                return mipPadding;
            }
            set
            {
                mipPadding = value;
            }
        }

        public ushort[] Misc
        {
            get
            {
                return misc;
            }
            set
            {
                misc = value;
            }
        }


        public FshLoadBitmapItem()
        {
        }

        public FshLoadBitmapItem(int width, int height) 
        {            
            this.disposed = false;
            this.surface = new Surface(width, height);
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
                disposed = true;

                if (disposing)
                {
                    if (surface != null)
                    {
                        surface.Dispose();
                        surface = null;
                    }
                }                  
            }
        }

    }
}
