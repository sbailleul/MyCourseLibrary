﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CourseLibrary.API.Helpers
{
    public class ArrayModelBinder: IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (!bindingContext.ModelMetadata.IsEnumerableType)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;;
            }

            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).ToString();

            if (string.IsNullOrWhiteSpace(value))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            var typedValues = GetTypedValues(bindingContext, value);
            bindingContext.Model = typedValues;
            
            bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
            return Task.CompletedTask;            
        }

        private static Array GetTypedValues(ModelBindingContext bindingContext, string value)
        {
            var elementType = bindingContext.ModelType.GetTypeInfo().GenericTypeArguments[0];
            var converter = TypeDescriptor.GetConverter(elementType);
            var values = value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(v => converter.ConvertFromString(v.Trim())).ToArray();

            var typedValues = Array.CreateInstance(elementType, values.Length);
            values.CopyTo(typedValues, 0);
            return typedValues;
        }
    }
}