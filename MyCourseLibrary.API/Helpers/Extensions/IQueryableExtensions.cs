using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using CourseLibrary.API.Services.PropertyMapping;

namespace CourseLibrary.API.Helpers.Extensions
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string orderBy,
            Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (mappingDictionary == null) throw new ArgumentNullException(nameof(mappingDictionary));
            if (string.IsNullOrWhiteSpace(orderBy)) return source;

            var orderByAfterSplit = orderBy.Split(',');
            
            foreach (var clause in orderByAfterSplit)
            {
                var trimmedClause = clause.Trim();
                var orderDescending = trimmedClause.EndsWith(" desc");
                var indexFirstSpace = trimmedClause.IndexOf(' ');
                var propertyName = indexFirstSpace == -1 ? trimmedClause : trimmedClause.Remove(indexFirstSpace);

                if (!mappingDictionary.TryGetValue(propertyName, out var propertyMappingValue))
                {
                    throw new ArgumentException($"Key mapping for {propertyName} is missing");
                }

                if (propertyMappingValue == null)
                {
                    throw new ArgumentNullException(nameof(propertyMappingValue));
                }
                
                foreach (var destinationProperty in propertyMappingValue.DestinationProperties.Reverse())
                {
                    if (propertyMappingValue.Revert)
                    {
                        orderDescending = !orderDescending;
                    }

                    source = source.OrderBy(destinationProperty + (orderDescending ? " descending" : " ascending"));
                }
            }

            return source;
        }
    }
}