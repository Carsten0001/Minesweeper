using System.Globalization;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Data;

namespace Minesweeper.Validators
{
    /// <summary>
    /// Checks the Height Property of Game Field
    /// </summary>
    public class FieldHeightWidthRule : ValidationRule
    {
        /// <summary>
        /// Min Bounds
        /// </summary>
        public int Min { get; set; }

        /// <summary>
        /// MaxBounds
        /// </summary>
        public int Max { get; set; }

        /// <summary>
        /// Checks if values is valid.
        /// </summary>
        /// <param name="value">The value that should be validated</param>
        /// <param name="cultureInfo">Not used</param>
        /// <returns>ValidationResult</returns>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int sizeXY = (int)GetBoundValue(value);

            if ((sizeXY < Min) || (sizeXY > Max))
            {
                return new ValidationResult(false,
                  $"Please enter a number in the range: { Min } - { Max } .");
            }
            else
            {
                return ValidationResult.ValidResult;
            }
        }

        private static object GetBoundValue(object value)
        {
            // ValidationStep was UpdatedValue or CommittedValue (Validate after setting)
            // Need to pull the value out of the BindingExpression.
            if (value is BindingExpression binding)
            {
                // Get the bound object and name of the property
                string resolvedPropertyName = binding.GetType().GetProperty("ResolvedSourcePropertyName", BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance).GetValue(binding, null).ToString();
                object resolvedSource = binding.GetType().GetProperty("ResolvedSource", BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance).GetValue(binding, null);

                // Extract the value of the property
                object propertyValue = resolvedSource.GetType().GetProperty(resolvedPropertyName).GetValue(resolvedSource, null);

                // This is what we want.
                return propertyValue;
            }
            else
            {
                // ValidationStep was RawProposedValue or ConvertedProposedValue
                // The argument is already what we want!
                return value;
            }
        }
    }
}