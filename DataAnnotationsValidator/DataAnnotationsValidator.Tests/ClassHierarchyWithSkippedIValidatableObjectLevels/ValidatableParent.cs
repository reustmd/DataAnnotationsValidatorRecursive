using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAnnotationsValidator.Tests.ClassHierarchyWithSkippedIValidatableObjectLevels
{
	public class ValidatableParent : NonValidatableChild, IValidatableObject
	{
		public bool ValidatableParentIsValid { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (!ValidatableParentIsValid)
				yield return new ValidationResult("ValidatableParent is invalid");
		}
	}
}
