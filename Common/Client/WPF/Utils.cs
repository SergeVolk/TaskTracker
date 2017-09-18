using System;
using System.Collections.Generic;

namespace TaskTracker.Client.WPF.Utils
{
    public static class EnumUtils
    {
        public static IEnumerable<T> GetValues<T>()
        {
            foreach (var item in Enum.GetValues(typeof(T)))
            {
                yield return (T)item;
            }
        }       
    }

    public static class EnumerableUtils
    {
        public static void ForEach<T>(this IEnumerable<T> target, Action<T> action)
        {
            foreach (var item in target)
            {
                action(item);
            }
        }
    }
}
