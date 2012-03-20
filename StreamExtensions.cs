using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FSHfiletype
{
    static class StreamExtensions
    {

        public static int ReadInt32(this Stream s)
        {
            int byte0 = s.ReadByte();

            if (byte0 == -1)
            {
                throw new EndOfStreamException();
            }
            int byte1 = s.ReadByte();

            if (byte1 == -1)
            {
                throw new EndOfStreamException();
            }
            int byte2 = s.ReadByte();

            if (byte2 == -1)
            {
                throw new EndOfStreamException();
            }
            int byte3 = s.ReadByte();

            if (byte3 == -1)
            {
                throw new EndOfStreamException();
            }

            return (int)((byte3 << 24) | (byte2 << 16) | (byte1 << 8) | byte0);
        }

        public static ushort ReadUInt16(this Stream s)
        {
            int byte0 = s.ReadByte();

            if (byte0 == -1)
            {
                throw new EndOfStreamException();
            }
            int byte1 = s.ReadByte();

            if (byte1 == -1)
            {
                throw new EndOfStreamException();
            }

            return (ushort)((byte1 << 8) | byte0);
        }
    }
}
