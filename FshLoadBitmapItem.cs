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
