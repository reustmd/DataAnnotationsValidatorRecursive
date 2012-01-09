using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DataAnnotationsValidator
{
	public class DataAnnotationsValidator : IDataAnnotationsValidator
	{
		public bool TryValidateObject(object obj, ICollection<ValidationResult> results)
		{
			return Validator.TryValidateObject(obj, new ValidationContext(obj, null, null), results, true);
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
