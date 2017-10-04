using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTracker.ExceptionUtils
{   
    /// <summary>
    /// Helper class for typical cases of argument validation.
    /// </summary>
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

        /// <summary>
        /// Throws ArgumentException if the input condition is false.
        /// </summary>
        /// <param name="exceptionParametersProvider">
        /// Callback that provide parameters for the exception. It is called only when the condition is false.
        /// </param>
        public static void ThrowIf(bool throwCondition, Func<IEnumerable<object>> exceptionParametersProvider)
        {
            Throw.If<ArgumentException>(throwCondition, exceptionParametersProvider);
        }

        /// <summary>
        /// Throws exception of the provided type if the input condition is false.
        /// </summary>
        /// <typeparam name="E">Exception to be thrown</typeparam>
        /// <param name="exceptionParametersProvider">
        /// Callback that provide parameters for the exception. It is called only when the condition is false.
        /// </param>
        public static void ThrowIf<E>(bool throwCondition, Func<IEnumerable<object>> exceptionParametersProvider) where E : ArgumentException
        {
            Throw.If<E>(throwCondition, exceptionParametersProvider);
        }

        /// <summary>
        /// Throws ArgumentException if the input enumerable is null or empty.
        /// </summary>
        /// <param name="objectName">Argument name</param>
        public static void ThrowIfNullOrEmpty<T>(IEnumerable<T> enumerableObject, string objectName)
        {
            Throw.IfNullOrEmpty<ArgumentException, T>(enumerableObject, $"Argument '{objectName}' cannot be null or empty.", objectName);
        }

        /// <summary>
        /// Throws ArgumentNullException if the input object is null.
        /// </summary>
        /// <param name="objectName">Argument name</param>
        public static void ThrowIfNull(object @object, string objectName)
        {
            Throw.IfNull<ArgumentNullException>(@object, objectName);
        }

        /// <summary>
        /// Throws ArgumentOutOfRangeException if the input value is strictly less than the provided minimum.
        /// </summary>
        /// <param name="objectName">Argument name</param>
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

        /// <summary>
        /// Throws ArgumentOutOfRangeException if the input value is strictly greater than the provided maximim.
        /// </summary>
        /// <param name="objectName">Argument name</param>
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

        /// <summary>
        /// Throws ArgumentOutOfRangeException if the input value is not exclusively inside the range (min; max). 
        /// </summary>
        /// <param name="objectName">Argument name</param>
        public static void ThrowIfOutOfRange<T>(T value, T min, T max, string objectName)
        {
            Throw.IfOutOfRange<ArgumentOutOfRangeException, T>(value, min, max, objectName);                
        }
    }
}
