using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSHfiletype
{
    internal struct FSHHeader
    {
        public byte[] SHPI;
        public int size;
        public int numBmps;
        public byte[] dirID;
    }

    internal struct FSHDirEntry
    {
        public byte[] name;
        public int offset;

        internal FSHDirEntry(byte[] name)
        {
            this.name = name;
            this.offset = 0;
        }
    }

    internal struct FSHEntryHeader
    {
        public int code;
        public ushort width;
        public ushort height;
        public ushort[] misc;
    }
    
}
