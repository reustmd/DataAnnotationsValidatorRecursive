using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAnnotationsValidator.Tests.SimpleClassHierarchy
{
	public class BaseParent : IValidatableObject
	{
		public BaseParent()
		{
			BasePropertyA = 2;
		}

		[Required(ErrorMessage = "Parent PropertyA is required")]
		[Range(1, 10, ErrorMessage = "Parent PropertyA not within range")]
		public int BasePropertyA { get; set; }

		IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
		{
			if (BasePropertyA % 2 != 0)
				yield return new ValidationResult("BaseParent PropertyA must be divisible by 2");
		}
	}
}
