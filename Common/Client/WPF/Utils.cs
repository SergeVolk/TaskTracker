using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static IEnumerable<TOutputItem> ConvertValues<TEnum, TOutputItem>(Converter<TEnum, TOutputItem> converter)
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            var result = new List<TOutputItem>();
            foreach (var item in GetValues<TEnum>())
            {
                result.Add(converter(item));
            };
            return result;
        }        
    }

    public static class EnumerableUtils
    {
        public static List<TResult> Convert<TResult, TSource>(this IEnumerable<TSource> source, Converter<TSource, TResult> converter)
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            var result = new List<TResult>();
            foreach (var item in source)
            {
                result.Add(converter(item));
            }
            return result;
        }

        public static void ForEach<T>(this IEnumerable<T> target, Action<T> action)
        {
            foreach (var item in target)
            {
                action(item);
            }
        }
    }
}
