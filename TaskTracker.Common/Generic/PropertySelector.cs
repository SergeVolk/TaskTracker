using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace TaskTracker.SyntaxUtils
{   
    /// <summary>
    /// This class is used to "mark" (or select) properties of a class in a useful manner 
    /// and return them as a list of strings.
    /// It is used to inform the Repository which properties are required to be intialized.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    [DataContract]
    public class PropertySelector<T>
    {
        [DataMember]
        private List<string> properties;

        public PropertySelector()
        {
            properties = new List<string>();
        }
                
        public PropertySelector<T> Select(string propertyPath)
        {
            if (String.IsNullOrEmpty(propertyPath))
                throw new ArgumentException($"Argument '{nameof(propertyPath)}' cannot be null or empty.");

            Type parent = typeof(T);            

            foreach (var prop in propertyPath.Split('.'))
            {
                PropertyInfo info = parent.GetRuntimeProperty(prop);

                if (info == null)
                    throw new InvalidOperationException($"Property '{prop}' does not exists.");
                
                Type propType = info.PropertyType;

                if (!IsCollection(propType, out parent))
                    parent = propType;                
            }

            properties.Add(propertyPath);

            return this;
        }

        public PropertySelector<T> Select<TProperty>(Expression<Func<T, TProperty>> property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            var member = property.Body as MemberExpression;
            if (member == null || member.Member.MemberType != MemberTypes.Property || member.Expression.NodeType != ExpressionType.Parameter)
                throw new InvalidOperationException("Provided expression cannot be used for selecting the property.");

            properties.Add(member.Member.Name);            

            return this;
        }

        public IEnumerable<string> GetProperties()
        {
            return properties; 
        } 

        private static bool IsCollection(Type type, out Type itemType)
        {
            itemType = null;

            if (IsGenericEnumerable(type))
            {
                itemType = type.GetGenericArguments()[0];
            }
            else if (type.IsArray)
            {
                itemType = type.GetElementType();
            }
            else
            {
                var types = type.GetInterfaces().Where(x => IsGenericEnumerable(x)).ToArray();
                if (types.Length == 1)
                {
                    itemType = types[0].GetGenericArguments()[0];
                }
                else
                {
                    itemType = type.GetProperty("Item")?.PropertyType;
                }
            }           

            return itemType != null;
        }

        private static bool IsGenericEnumerable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }
    }
}
