using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NUnit.Framework;

namespace DataAnnotationsValidator.Tests
{
	[TestFixture]
	public class DataAnnotationsValidatorTests
	{
		[Test]
		public void TryValidateObject_on_valid_parent_returns_no_errors()
		{
			var parent = new Parent {PropertyA = 1, PropertyB = 1};
			var validationResults = new List<ValidationResult>();

			var result = DataAnnotationsValidator.TryValidateObject(parent, validationResults);

			Assert.IsTrue(result);
			Assert.AreEqual(0, validationResults.Count);
		}

		[Test]
		public void TryValidateObject_when_missing_required_properties_returns_errors()
		{
			var parent = new Parent { PropertyA = null, PropertyB = null };
			var validationResults = new List<ValidationResult>();

			var result = DataAnnotationsValidator.TryValidateObject(parent, validationResults);

			Assert.IsFalse(result);
			Assert.AreEqual(2, validationResults.Count);
			Assert.AreEqual(1, validationResults.ToList().Count(x => x.ErrorMessage == "Parent PropertyA is required"));
			Assert.AreEqual(1, validationResults.ToList().Count(x => x.ErrorMessage == "Parent PropertyB is required"));
		}

		[Test]
		public void TryValidateObject_calls_IValidatableObject_method()
		{
			var parent = new Parent { PropertyA = 5, PropertyB = 6 };
			var validationResults = new List<ValidationResult>();

			var result = DataAnnotationsValidator.TryValidateObject(parent, validationResults);

			Assert.IsFalse(result);
			Assert.AreEqual(1, validationResults.Count);
			Assert.AreEqual("Parent PropertyA and PropertyB cannot add up to more than 10", validationResults[0].ErrorMessage);
		}

		[Test]
		public void TryValidateObjectRecursive_returns_errors_when_child_class_has_invalid_properties()
		{
			var parent = new Parent { PropertyA = 1, PropertyB = 1 };
			parent.Child = new Child{PropertyA = null, PropertyB = 5};
			var validationResults = new List<ValidationResult>();

			var result = DataAnnotationsValidator.TryValidateObjectRecursive(parent, validationResults);

			Assert.IsFalse(result);
			Assert.AreEqual(1, validationResults.Count);
			Assert.AreEqual("Child PropertyA is required", validationResults[0].ErrorMessage);
		}

		[Test]
		public void TryValidateObjectRecursive_calls_IValidatableObject_method_on_child_class()
		{
			var parent = new Parent { PropertyA = 1, PropertyB = 1 };
			parent.Child = new Child { PropertyA = 5, PropertyB = 6 };
			var validationResults = new List<ValidationResult>();

			var result = DataAnnotationsValidator.TryValidateObjectRecursive(parent, validationResults);

			Assert.IsFalse(result);
			Assert.AreEqual(1, validationResults.Count);
			Assert.AreEqual("Child PropertyA and PropertyB cannot add up to more than 10", validationResults[0].ErrorMessage);
		}

		[Test]
		public void TryValidateObjectRecursive_returns_errors_when_grandchild_class_has_invalid_properties()
		{
			var parent = new Parent { PropertyA = 1, PropertyB = 1 };
			parent.Child = new Child { PropertyA = 1, PropertyB = 1 };
			parent.Child.GrandChildren = new [] {new GrandChild{PropertyA = 11, PropertyB = 11}};
			var validationResults = new List<ValidationResult>();

			var result = DataAnnotationsValidator.TryValidateObjectRecursive(parent, validationResults);

			Assert.IsFalse(result);
			Assert.AreEqual(2, validationResults.Count);
			Assert.AreEqual(1, validationResults.ToList().Count(x => x.ErrorMessage == "GrandChild PropertyA not within range"));
			Assert.AreEqual(1, validationResults.ToList().Count(x => x.ErrorMessage == "GrandChild PropertyB not within range"));
		}

		[Test]
		public void TryValidateObject_calls_grandchild_IValidatableObject_method()
		{
			var parent = new Parent { PropertyA = 1, PropertyB = 1 };
			parent.Child = new Child { PropertyA = 1, PropertyB = 1 };
			parent.Child.GrandChildren = new[] { new GrandChild { PropertyA = 5, PropertyB = 6 } };
			var validationResults = new List<ValidationResult>();

			var result = DataAnnotationsValidator.TryValidateObjectRecursive(parent, validationResults);

			Assert.IsFalse(result);
			Assert.AreEqual(1, validationResults.Count);
			Assert.AreEqual(1, validationResults.ToList().Count(x => x.ErrorMessage == "GrandChild PropertyA and PropertyB cannot add up to more than 10"));
		}
	}
}
