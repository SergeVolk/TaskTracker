using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskTracker.SyntaxUtils
{
    public static class EnumerableUtils
    {
        public static void ForEach<T>(this IEnumerable<T> target, Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            foreach (var item in target)
            {
                action(item);
            }
        }
    }
}
