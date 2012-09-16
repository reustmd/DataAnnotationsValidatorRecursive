using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAnnotationsValidator.Tests.ClassHierarchyWithSkippedIValidatableObjectLevels
{
	public class ValidatableGrandChild : IValidatableObject
	{
		public bool ValidatableGrandChildIsValid { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (!ValidatableGrandChildIsValid)
				yield return new ValidationResult("ValidatableGrandChild is invalid");
		}
	}
}
