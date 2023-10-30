using System;
using System.Linq;

namespace Rito.CUT
{
    public static class ArrayExtension
    {
        public static void Foreach<T>(this T[] @this, Action<T> action)
        {
            if (@this == null || action == null) return;
            int length = @this.Length;
            for (int i = 0; i < length; i++)
            {
                action.Invoke(@this[i]);
            }
        }

        public static TResult[] Map<T, TResult>(this T[] @this, Func<T, TResult> func)
        {
            return @this.Select(func).ToArray();
        }

        public static TResult[] Map<T, TResult>(this T[] @this, Func<T, int, TResult> func)
        {
            return @this.Select(func).ToArray();
        }
    }
}