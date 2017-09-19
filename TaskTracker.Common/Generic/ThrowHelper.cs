using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTracker.ExceptionUtils
{   
    public static class ArgumentValidation 
    {
        private static class Throw
        {
            public static void If<E>(bool throwCondition, params object[] exceptionParameters) where E : Exception
            {
                Throw.If<E>(throwCondition, () => exceptionParameters);
            }

            public static void IfNull<E>(object @object, params object[] exceptionParameters) where E : Exception
            {
                Throw.If<E>(@object == null, exceptionParameters);
            }

            public static void IfNullOrEmpty<E, T>(IEnumerable<T> enumerableObject, params object[] parameters) where E : Exception
            {
                Throw.If<E>(enumerableObject == null || !enumerableObject.Any(), parameters);
            }

            public static void IfOutOfRange<E, T>(T value, T min, T max, string objectName) where E : Exception
            {
                Throw.If<E>(
                    Comparer<T>.Default.Compare(value, min) < 0 ||
                    Comparer<T>.Default.Compare(value, max) > 0,
                    () => (new[]
                    {
                        objectName,
                        $"Object '{objectName}' must be in range [{min.ToString()};{max.ToString()}]. Current value is '{value.ToString()}'."
                    }));
            }

            public static void If<E>(bool throwCondition, Func<IEnumerable<object>> exceptionParametersProvider) where E : Exception
            {
                if (throwCondition)
                {
                    var types = new List<Type>();
                    var args = new List<object>();

                    if (exceptionParametersProvider != null)
                    {
                        foreach (object p in exceptionParametersProvider())
                        {
                            types.Add(p.GetType());
                            args.Add(p);
                        }
                    }

                    var constructor = typeof(E).GetConstructor(types.ToArray());
                    var exception = constructor.Invoke(args.ToArray()) as E;
                    throw exception;
                }
            }
        }

        public static void ThrowIf(bool throwCondition, Func<IEnumerable<object>> exceptionParametersProvider)
        {
            Throw.If<ArgumentException>(throwCondition, exceptionParametersProvider);
        }

        public static void ThrowIf<E>(bool throwCondition, Func<IEnumerable<object>> exceptionParametersProvider) where E : ArgumentException
        {
            Throw.If<E>(throwCondition, exceptionParametersProvider);
        }

        public static void ThrowIfNullOrEmpty<T>(IEnumerable<T> enumerableObject, string objectName)
        {
            Throw.IfNullOrEmpty<ArgumentException, T>(enumerableObject, $"Argument '{objectName}' cannot be null or empty.", objectName);
        }

        public static void ThrowIfNull(object @object, string objectName)
        {
            Throw.IfNull<ArgumentNullException>(@object, objectName);
        }

        public static void ThrowIfLess<T>(T value, T min, string objectName)
        {
            Throw.If<ArgumentOutOfRangeException>(
                Comparer<T>.Default.Compare(value, min) < 0,
                () => (new[]
                {
                    objectName,
                    $"Argument '{objectName}' cannot be less than '{min.ToString()}'. Current value is '{value.ToString()}'."
                }));
        }

        public static void ThrowIfGreater<T>(T value, T max, string objectName)
        {
            Throw.If<ArgumentOutOfRangeException>(
                Comparer<T>.Default.Compare(value, max) > 0,
                () => (new[]
                {
                    objectName,
                    $"Argument '{objectName}' cannot be greater than '{max.ToString()}'. Current value is '{value.ToString()}'."
                }));
        }

        public static void ThrowIfOutOfRange<T>(T value, T min, T max, string objectName)
        {
            Throw.IfOutOfRange<ArgumentOutOfRangeException, T>(value, min, max, objectName);                
        }
    }
}
