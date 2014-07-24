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
