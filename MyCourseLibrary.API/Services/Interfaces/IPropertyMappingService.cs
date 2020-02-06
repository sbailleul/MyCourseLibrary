using System.Collections.Generic;
using CourseLibrary.API.Services.PropertyMapping;

namespace CourseLibrary.API.Services.Interfaces
{
    public interface IPropertyMappingService
    {
        Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>();
        bool ValidMappingExistFor<TSource, TDestination>(string fields);
    }
}