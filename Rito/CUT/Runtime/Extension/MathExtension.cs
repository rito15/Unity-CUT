using System;

namespace Rito.CUT
{
    public static class MathExtension
    {
        /***********************************************************************
        *                           int - Range, Clamp
        ***********************************************************************/
        #region .
        /// <summary> (min &lt;= value &lt;= max) </summary>
        public static bool InRange(in this int value, in int min, in int max)
            => min <= value && value <= max;

        /// <summary> (min &lt; value &lt; max) </summary>
        public static bool ExRange(in this int value, in int min, in int max)
            => min < value && value < max;

        /// <summary> (min &lt;= value &lt; max) </summary>
        public static bool InExRange(in this int value, in int min, in int max)
            => min <= value && value < max;

        /// <summary> (min &lt; value &lt;= max) </summary>
        public static bool ExInRange(in this int value, in int min, in int max)
            => min < value && value <= max;


        /// <summary> 값의 범위 제한 </summary>
        public static int Clamp(in this int value, in int min, in int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
        /// <summary> 값의 최소 범위 제한 </summary>
        public static int ClampMin(in this int value, in int min)
        {
            if (value < min) return min;
            return value;
        }
        /// <summary> 값의 최대 범위 제한 </summary>
        public static int ClampMax(in this int value, in int max)
        {
            if (value > max) return max;
            return value;
        }


        /// <summary> 값의 범위 제한 </summary>
        public static int ClampRef(ref this int value, in int min, in int max)
        {
            if (value < min) value = min;
            if (value > max) value = max;
            return value;
        }
        /// <summary> 값의 최소 범위 제한 </summary>
        public static int ClampMinRef(ref this int value, in int min)
        {
            if (value < min) value = min;
            return value;
        }
        /// <summary> 값의 최대 범위 제한 </summary>
        public static int ClampMaxRef(ref this int value, in int max)
        {
            if (value > max) value = max;
            return value;
        }
        #endregion

        /***********************************************************************
        *                           float - Range, Clamp
        ***********************************************************************/
        #region .
        /// <summary> (min &lt;= value &lt;= max) </summary>
        public static bool InRange(in this float value, in float min, in float max)
            => min <= value && value <= max;

        /// <summary> (min &lt; value &lt; max) </summary>
        public static bool ExRange(in this float value, in float min, in float max)
            => min < value && value < max;

        /// <summary> (min &lt;= value &lt; max) </summary>
        public static bool InExRange(in this float value, in float min, in float max)
            => min <= value && value < max;

        /// <summary> (min &lt; value &lt;= max) </summary>
        public static bool ExInRange(in this float value, in float min, in float max)
            => min < value && value <= max;


        /// <summary> 값의 범위 제한 </summary>
        public static float Clamp(in this float value, in float min, in float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
        /// <summary> 값의 최소 범위 제한 </summary>
        public static float ClampMin(in this float value, in float min)
        {
            if (value < min) return min;
            return value;
        }
        /// <summary> 값의 최대 범위 제한 </summary>
        public static float ClampMax(in this float value, in float max)
        {
            if (value > max) return max;
            return value;
        }


        /// <summary> 값의 범위 제한 </summary>
        public static float ClampRef(ref this float value, in float min, in float max)
        {
            if (value < min) value = min;
            if (value > max) value = max;
            return value;
        }
        /// <summary> 값의 최소 범위 제한 </summary>
        public static float ClampMinRef(ref this float value, in float min)
        {
            if (value < min) value = min;
            return value;
        }
        /// <summary> 값의 최대 범위 제한 </summary>
        public static float ClampMaxRef(ref this float value, in float max)
        {
            if (value > max) value = max;
            return value;
        }


        /// <summary> 0 ~ 1 범위로 제한 </summary>
        public static float Saturate(in this float value)
        {
            if (value < 0f) return 0f;
            if (value > 1f) return 1f;
            return value;
        }

        /// <summary> 0 ~ 1 범위로 제한 </summary>
        public static void SaturateRef(ref this float value)
        {
            if (value < 0f) value = 0f;
            if (value > 1f) value = 1f;
        }

        #endregion

        /***********************************************************************
        *                           float
        ***********************************************************************/
        #region .

        /// <summary> 범위값 변경 </summary>
        public static float Remap(in this float value, float oldMin, float oldMax, float newMin, float newMax)
        {
            return newMin + ((value - oldMin) * (newMax - newMin) /
                                                (oldMax - oldMin));
        }

        /// <summary> 부호값 (-1, 0, +1) </summary>
        public static float Sign(in this float value)
        {
            return (value > 0f) ? +1f : 
                   (value < 0f) ? -1f : 
                   0f;
        }

        #endregion
    }
}