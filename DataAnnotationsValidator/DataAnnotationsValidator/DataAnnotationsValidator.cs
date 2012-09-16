using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace DataAnnotationsValidator
{
	public class DataAnnotationsValidator : IDataAnnotationsValidator
	{
		public bool TryValidateObject(object obj, ICollection<ValidationResult> results)
		{
			return Validator.TryValidateObject(obj, new ValidationContext(obj, null, null), results, true);
		}

		public bool TryValidateObjectRecursiveIncludingBaseTypes<T>(T obj, List<ValidationResult> results)
		{
			var typesInHierarchy = new List<Type> {obj.GetType()};
			var baseType = obj.GetType().BaseType;
			
			while (baseType != null)
			{
				typesInHierarchy.Add(baseType);
				baseType = baseType.BaseType;
			}

			//validate the root object and all properties in the hierarchy chain
			var result = TryValidateObjectRecursive(obj, results);
			
			//need to explicitly call Validate on all base types, after the initial Validate which was invoked by TryValidateObjectRecursive
			foreach (var type in typesInHierarchy)
			{
				MethodInfo castMethod = GetType().GetMethod("Cast").MakeGenericMethod(type);
				object castedObject = castMethod.Invoke(null, new object[] { obj });
				var validatable = castedObject as IValidatableObject;

				if (validatable != null)
				{
					var feaf = validatable.Validate(null);	
				}
			}

			return result;
		}

		public static T Cast<T>(object o)
		{
			return (T)o;
		}

		public bool TryValidateObjectRecursive<T>(T obj, List<ValidationResult> results)
		{
			bool result = TryValidateObject(obj, results);

			var properties = obj.GetType().GetProperties().Where(prop => prop.CanRead && !prop.GetCustomAttributes(typeof(SkipRecursiveValidation), false).Any()).ToList();

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
						result = TryValidateObjectRecursive(enumObj, results) && result;
					}
				}
				else
				{
					result = TryValidateObjectRecursive(value, results) && result;
				}
			}

			return result;
		}
	}
}
