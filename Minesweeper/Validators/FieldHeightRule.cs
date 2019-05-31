using System;
using System.Globalization;
using System.Windows.Controls;

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
            int sizeY = 0;

            try
            {
                if (((string)value).Length > 0)
                    sizeY = int.Parse((string)value);
            }
            catch (Exception e)
            {
                return new ValidationResult(false, $"Illegal characters or { e.Message }");
            }

            if ((sizeY < Min) || (sizeY > Max))
            {
                return new ValidationResult(false,
                  $"Please enter a number in the range: { Min } - { Max } .");
            }
            else
            {
                return ValidationResult.ValidResult;
            }
        }
    }
}
