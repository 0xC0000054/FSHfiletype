using System;
using PaintDotNet;

namespace FSHfiletype
{
    internal sealed class FshLoadBitmapItem : IDisposable
    {
        private Surface surface;
        private FshMetadata metaData;


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

        public FshMetadata MetaData
        {
            get
            {
                return metaData;
            }
            set
            {
                metaData = value;
            }
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
