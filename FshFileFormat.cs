
namespace FSHfiletype
{
    internal enum FshFileFormat 
    {
        /// <summary>
        /// 24-bit RGB (0:8:8:8)
        /// </summary>
        TwentyFourBit,
        /// <summary>
        /// 32-bit ARGB (8:8:8:8)
        /// </summary>
        ThirtyTwoBit,
        /// <summary>
        /// 16-bit RGB (0:5:5:5)
        /// </summary>
        SixteenBit,
        /// <summary>
        /// 16-bit ARGB (1:5:5:5)
        /// </summary>
        SixteenBitAlpha,
        /// <summary>
        /// 16-bit ARGB (4:4:4:4)
        /// </summary>
        SixteenBit4x4,
        /// <summary>
        /// DXT1 4x4 block compression  
        /// </summary>
        DXT1,
        /// <summary>
        /// DXT3 4x4 block compression  
        /// </summary>
        DXT3
    }


}
