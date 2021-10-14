using Minesweeper.Model;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Minesweeper.Converters
{
    /// <summary>
    /// A Converter for a bool and a <see cref="GameMode"/>
    /// </summary>
    internal class GameModeToBooleanConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts bool and GameMode to one bool output.
        /// </summary>
        /// <param name="values">Array of two input values. First index must be a bool the second a GameMode</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">the string "Difficulty" or null</param>
        /// <param name="culture">not used</param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((string)parameter == "Difficulty")
            {
                if ((bool)values[0] == false || (GameMode)values[1] == GameMode.Custom)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                if ((bool)values[0] == false || (GameMode)values[1] == GameMode.Standard)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Returns always null
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetTypes"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>null</returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}