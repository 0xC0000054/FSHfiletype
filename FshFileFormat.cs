using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSHfiletype
{
    internal enum FshFileFormat : byte
    {
        TwentyFourBit = 0x7f,
        ThirtyTwoBit = 0x7d,
        DXT1 = 0x60,
        DXT3 = 0x61
    }


}
