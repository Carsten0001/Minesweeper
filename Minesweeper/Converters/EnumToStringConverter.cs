using Minesweeper.Properties;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Minesweeper.Converters
{
    /// <summary>
    /// Converts Enum values to localized strings and back.
    /// </summary>
    public class EnumToStringConverter : IValueConverter
    {
        /// <summary>
        /// Converts enum value to localized string.
        /// </summary>
        /// <param name="value">The enum value</param>
        /// <param name="targetType">String</param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>Localized string of enum value</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? null : Resources.ResourceManager.GetString(value.ToString(), CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="value">String</param>
        /// <param name="targetType">Enum type</param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>Enum value</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = (string)value;

            foreach (object enumValue in Enum.GetValues(targetType))
            {
                if (str == Resources.ResourceManager.GetString(enumValue.ToString(), CultureInfo.CurrentCulture))
                    return enumValue;
            }

            throw new ArgumentException(null, nameof(value));
        }
    }
}
