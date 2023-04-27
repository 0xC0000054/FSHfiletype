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
