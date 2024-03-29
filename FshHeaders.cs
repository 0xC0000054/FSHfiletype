﻿/*
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
using System.IO;
using System.Text;

namespace FSHfiletype
{
    internal sealed class FSHHeader
    {
        private int size;
        private int imageCount;
        private byte[] dirID;

        /// <summary>
        /// The FSH file signature - SHPI
        /// </summary>
        private const uint FSHSignature = 0x49504853U; 
        internal const int SizeOf = 16;

        public int Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
            }
        }
        
        public int ImageCount
        {
            get
            {
                return imageCount;
            }
        }

        public FSHHeader(Stream stream)
        {
            if (stream.ReadUInt32() != FSHSignature)
            {
                throw new FileFormatException(Properties.Resources.InvalidFshHeader);
            }
            this.size = stream.ReadInt32();
            this.imageCount = stream.ReadInt32();
            this.dirID = new byte[4];
            stream.ProperRead(this.dirID, 0, 4);
        }

        public FSHHeader(int imageCount, string dirID)
        {
            this.size = 0;
            this.imageCount = imageCount;
            this.dirID = Encoding.ASCII.GetBytes(dirID);
        }

        public void Save(Stream stream)
        {
            stream.WriteUInt32(FSHSignature);
            stream.WriteInt32(this.size);
            stream.WriteInt32(this.imageCount);
            stream.Write(this.dirID, 0, 4);
        }
    }

    internal sealed class FSHDirEntry
    {
        public byte[] name;
        public int offset;

        internal const int SizeOf = 8;

        internal FSHDirEntry(byte[] name)
        {
            this.name = name;
            this.offset = 0;
        }

        internal FSHDirEntry(Stream input)
        {
            this.name = new byte[4];
            input.ProperRead(name, 0, 4);
            this.offset = input.ReadInt32();
        }

        public void Save(Stream stream)
        {
            stream.Write(this.name, 0, 4);
            stream.WriteInt32(this.offset);
        }
    }

    [Serializable]
    internal sealed class FSHEntryHeader
    {
        public int code;
        public ushort width;
        public ushort height;
        public ushort[] misc;

        [NonSerialized]
        internal const int SizeOf = 16;

        public FSHEntryHeader()
        {
            this.code = 0;
            this.width = 0;
            this.height = 0;
            this.misc = new ushort[4];
        }

        internal FSHEntryHeader(Stream stream)
        {
            this.code = stream.ReadInt32();
            this.width = stream.ReadUInt16();
            this.height = stream.ReadUInt16();
            this.misc = new ushort[4];
            for (int m = 0; m < 4; m++)
            {
                this.misc[m] = stream.ReadUInt16();
            }
        }

        private FSHEntryHeader(FSHEntryHeader cloneMe)
        {
            this.code = cloneMe.code;
            this.width = cloneMe.width;
            this.height = cloneMe.height;
            this.misc = cloneMe.misc;
        }

        public FSHEntryHeader Clone()
        {
            return new FSHEntryHeader(this);
        }
    }
    
}
