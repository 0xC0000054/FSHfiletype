using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PaintDotNet;

namespace FSHfiletype
{
    class FshLoadBitmapItem : IDisposable
    {
        private Surface surface;
        private FshFileFormat format;

        public Surface Surface
        {
            get
            {
                return surface;
            }
            internal set
            {
                surface = value;
            }
        }

        public FshFileFormat Format
        {
            get
            {
                return format;
            }
            internal set
            {
                format = value;
            }
        }

        public FshLoadBitmapItem()
        {
            this.disposed = false;
            this.surface = null;
            this.format = FshFileFormat.DXT1;
        }

        public FshLoadBitmapItem(int width, int height, FshFileFormat format) : this()
        {
            this.surface = new Surface(width, height);
            this.format = format;
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
                    if (surface != null)
                    {
                        surface.Dispose();
                        surface = null;
                    }
                    disposed = true;
                }
            }
        }

    }
}
