﻿﻿using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace DataAnnotationsValidator
{
    public static class DataAnnotationsValidator : IDataAnnotationsValidator
    {
        public static bool TryValidateObject(object obj, ICollection<ValidationResult> results, IDictionary<object, object> validationContextItems = null)
        {
            return Validator.TryValidateObject(obj, new ValidationContext(obj, null, validationContextItems), results, true);
        }

        public static bool TryValidateObjectRecursive<T>(T obj, List<ValidationResult> results, IDictionary<object, object> validationContextItems = null)
        {
            return TryValidateObjectRecursive(obj, results, new HashSet<object>(), validationContextItems);
        }

        private static bool TryValidateObjectRecursive<T>(T obj, List<ValidationResult> results, ISet<object> validatedObjects, IDictionary<object, object> validationContextItems = null)
        {
            //short-circuit to avoid infinit loops on cyclical object graphs
            if (validatedObjects.Contains(obj))
            {
                return true;
            }

            validatedObjects.Add(obj);
            bool result = TryValidateObject(obj, results, validationContextItems);

            var properties = obj.GetType().GetProperties().Where(prop => prop.CanRead
                && !prop.GetCustomAttributes(typeof(SkipRecursiveValidationAttribute), false).Any()
                && prop.GetIndexParameters().Length == 0).ToList();

            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(string) || property.PropertyType.IsValueType) continue;

                var value = obj.GetPropertyValue(property.Name);

                if (value == null) continue;

                var asEnumerable = value as IEnumerable;
                if (asEnumerable != null)
                {
                    foreach (var enumObj in asEnumerable)
                    {
                        var nestedResults = new List<ValidationResult>();
                        if (!TryValidateObjectRecursive(enumObj, nestedResults, validatedObjects, validationContextItems))
                        {
                            result = false;
                            foreach (var validationResult in nestedResults)
                            {
                                PropertyInfo property1 = property;
                                results.Add(new ValidationResult(validationResult.ErrorMessage, validationResult.MemberNames.Select(x => property1.Name + '.' + x)));
                            }
                        };
                    }
                }
                else
                {
                    var nestedResults = new List<ValidationResult>();
                    if (!TryValidateObjectRecursive(value, nestedResults, validatedObjects, validationContextItems))
                    {
                        result = false;
                        foreach (var validationResult in nestedResults)
                        {
                            PropertyInfo property1 = property;
                            results.Add(new ValidationResult(validationResult.ErrorMessage, validationResult.MemberNames.Select(x => property1.Name + '.' + x)));
                        }
                    };
                }
            }

            return result;
        }
    }
}
