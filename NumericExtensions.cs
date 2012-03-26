using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
    
namespace FSHfiletype
{
    internal static class NumericExtensions
    {
        /// <summary>
        ///  Converts the UInt16 to an invariant culture string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string ToStringInvariant(this ushort value)
        {
            return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts the Int32 to an invariant culture string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string ToStringInvariant(this int value)
        {
            return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
