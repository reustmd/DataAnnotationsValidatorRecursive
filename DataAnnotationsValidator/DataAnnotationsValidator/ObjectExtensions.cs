using System.Reflection;

namespace DataAnnotationsValidator
{
	public static class ObjectExtensions
	{
		public static object GetPropertyValue(this object o, string propertyName)
		{
			object objValue = string.Empty;

            // First return only the property declared on the object's type.
			var propertyInfo = o.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            
            if (propertyInfo == null)
            {
                // fallback: try to get the property from the full hierarchy (may throw if ambiguous)
                propertyInfo = o.GetType().GetProperty(propertyName);
            }
            
			if (propertyInfo != null)
				objValue = propertyInfo.GetValue(o, null);

			return objValue;
		}
	}
}
