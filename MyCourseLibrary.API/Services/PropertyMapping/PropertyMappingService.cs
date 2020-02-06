using System;
using System.Collections.Generic;
using System.Linq;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Models.Authors;
using CourseLibrary.API.Services.Interfaces;

namespace CourseLibrary.API.Services.PropertyMapping
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private readonly Dictionary<string, PropertyMappingValue> _authorPropertyMappingValues = new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
        {
            {"Id", new PropertyMappingValue(new List<string>(){"Id"})},
            {"MainCategory", new PropertyMappingValue(new List<string>(){"MainCategory"})},
            {"Age", new PropertyMappingValue(new List<string>(){"DateOfBirth"}, true)},
            {"Name", new PropertyMappingValue(new List<string>(){"FirstName", "LastName"})},
        };
        
        private IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingService()
        {
            _propertyMappings.Add(new PropertyMapping<AuthorDto, Author>(_authorPropertyMappingValues));
        }

        public bool ValidMappingExistFor<TSource, TDestination>(string fields)
        {
            if (string.IsNullOrWhiteSpace(fields))
                return true;
            
            var propertyMapping = GetPropertyMapping<TSource, TDestination>();
            var fieldsAfterSplit = fields.Split(',');
            
            foreach (var field in fieldsAfterSplit)
            {
                var trimmedField = field.Trim();
                var indexOfFirstSpace = trimmedField.IndexOf(' ');
                var propertyName = indexOfFirstSpace == -1 ? trimmedField : trimmedField.Remove(indexOfFirstSpace);

                if (!propertyMapping.ContainsKey(propertyName))
                {
                    return false;
                }
            }

            return true;
        }

        public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
        {
            var matchingMapping = _propertyMappings.OfType<PropertyMapping<TSource, TDestination>>();
            if (matchingMapping.Count() == 1)
            {
                return matchingMapping.First().MappingDictionary;
            }
            throw new Exception($"Cannot find exact property mapping instance for <{typeof(TSource)},{typeof(TDestination)}>");
        }
    }
}