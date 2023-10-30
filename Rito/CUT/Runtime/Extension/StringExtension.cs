using System;

namespace Rito.CUT
{
    public static class StringExtension
    {
        public static bool IsNullOrEmpty(this string @this)
        {
            return @this == null || string.IsNullOrEmpty(@this);
        }
    }
}