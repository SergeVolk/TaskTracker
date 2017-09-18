using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace TaskTracker.Common
{   
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
            ArgumentValidation.ThrowIfNullOrEmpty(propertyPath, nameof(propertyPath));

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
            ArgumentValidation.ThrowIfNull(property, nameof(property));

            var member = property.Body as MemberExpression;
            if (member == null || member.Member.MemberType != MemberTypes.Property)
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

            var types = type.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)).ToArray();
            
            if (types.Length == 1)
            {
                itemType = types[0].GetGenericArguments()[0];
            }
            else if (type.IsArray)
            {
                itemType = type.GetElementType();
            }
            else
            {
                itemType = type.GetProperty("Item")?.PropertyType;
            }            

            return itemType != null;
        }
    }
}
