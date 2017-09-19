using System;
using System.Collections.Generic;

using TaskTracker.ExceptionUtils;

namespace TaskTracker.Presentation.WPF.Utils
{
    internal static class EnumUtils
    {
        public static IEnumerable<T> GetValues<T>()
        {
            foreach (var item in Enum.GetValues(typeof(T)))
            {
                yield return (T)item;
            }
        }       
    }

    internal static class EnumerableUtils
    {
        public static void ForEach<T>(this IEnumerable<T> target, Action<T> action)
        {
            ArgumentValidation.ThrowIfNull(action, nameof(action));

            foreach (var item in target)
            {
                action(item);
            }
        }
    }

    internal static class ConversionUtils
    {
        public static Double? SafeParseDouble(string str)
        {
            double tmp;
            return Double.TryParse(str, out tmp) ? tmp : (double?)null;
        }
    }
}
