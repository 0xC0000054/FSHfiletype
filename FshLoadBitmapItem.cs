/*
*  This file is part of fsh-filetype, a filetype plug-in for Paint.NET
*  that loads and saves FSH images.
*
*  Copyright (C) 2009, 2010, 2011, 2012, 2014, 2015, 2023 Nicholas Hayes
*
*  This program is free software: you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation, either version 3 of the License, or
*  (at your option) any later version.
*
*  This program is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with this program.  If not, see <http://www.gnu.org/licenses/>.
*
*/

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
