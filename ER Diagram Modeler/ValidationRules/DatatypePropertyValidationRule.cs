using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace ER_Diagram_Modeler.ValidationRules
{
	public class DatatypePropertyValidationRule: ValidationRule
	{
		public int MaxValue { get; set; }
		public int MinValue { get; set; }
		public string DatatypeName { get; set; }

		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			int val;

			if (!int.TryParse(value.ToString(), out val))
			{
				return new ValidationResult(false, $"{DatatypeName} has to be valid integer");
			}

			if (val < MinValue || val > MaxValue)
			{
				return new ValidationResult(false, $"{DatatypeName} value has to be between {MinValue} and {MaxValue}");
			}

			return new ValidationResult(true, null);
		}
	}
}