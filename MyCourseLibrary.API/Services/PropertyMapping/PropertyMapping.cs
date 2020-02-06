using System;
using System.Collections.Generic;
using CourseLibrary.API.Services.Interfaces;

namespace CourseLibrary.API.Services.PropertyMapping
{
    public class PropertyMapping<TSource, TDestination>: IPropertyMapping
    {
        public Dictionary<string, PropertyMappingValue> MappingDictionary { get; private set; }

        public PropertyMapping(Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            MappingDictionary = mappingDictionary ?? throw new ArgumentNullException(nameof(mappingDictionary));
        }
    }
}