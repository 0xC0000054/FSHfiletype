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

        public static uint ReadUInt32(this Stream s)
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

            return (uint)((byte3 << 24) | (byte2 << 16) | (byte1 << 8) | byte0);
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

        public static void WriteUInt16(this Stream s, ushort value)
        {
            s.WriteByte((byte)(value & 0xff));
            s.WriteByte((byte)((value >> 8) & 0xff));
        }

        public static void WriteUInt32(this Stream s, uint value)
        {
            s.WriteByte((byte)(value & 0xff));
            s.WriteByte((byte)((value >> 8) & 0xff));
            s.WriteByte((byte)((value >> 16) & 0xff));
            s.WriteByte((byte)((value >> 24) & 0xff));
        }

        public static void WriteInt32(this Stream s, int value)
        {
            s.WriteByte((byte)(value & 0xff));
            s.WriteByte((byte)((value >> 8) & 0xff));
            s.WriteByte((byte)((value >> 16) & 0xff));
            s.WriteByte((byte)((value >> 24) & 0xff));
        }

        public static void ProperRead(this Stream s, byte[] buffer, int offset, int count)
        {
            int numBytesToRead = count;
            int numBytesRead = 0;

            while (numBytesToRead > 0)
            {
                // Read may return anything from 0 to numBytesToRead.
                int bytesRead = s.Read(buffer, numBytesRead + offset, numBytesToRead);
                // The end of the file is reached.
                if (bytesRead == 0)
                {
                    break;
                }

                numBytesRead += bytesRead;
                numBytesToRead -= bytesRead;
            }
        }
    }
}
