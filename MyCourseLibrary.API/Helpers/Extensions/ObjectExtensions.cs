using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace CourseLibrary.API.Helpers.Extensions
{
    public static class ObjectExtensions
    {
         public static ExpandoObject ShapeData<TSource>(this TSource source, string fields = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var propertyInfoList = ReflectionHelper.GetPropertyInfoListByFields<TSource>(fields);
            
            return GetDataShappedObject(source, propertyInfoList);
        }

       

        private static ExpandoObject GetDataShappedObject<TSource>( TSource source, List<PropertyInfo> propertyInfoList)
        {
            var dataShapedObject = new ExpandoObject();
            foreach (var propertyInfo in propertyInfoList)
            {
                var propertyValue = propertyInfo.GetValue(source);
                ((IDictionary<string, object>) dataShapedObject).Add(propertyInfo.Name, propertyValue);
            }

            return dataShapedObject;
        }
    }
}