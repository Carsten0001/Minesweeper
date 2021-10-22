using Minesweeper.Properties;
using System;
using System.Windows.Markup;

namespace Minesweeper.MarkupExtensions
{
    /// <summary>
    /// MarkupExtension to Bind an enum
    /// </summary>
    public sealed class EnumerateExtension : MarkupExtension
    {
        /// <summary>
        /// Type of enum
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="type">Enum Type</param>
        public EnumerateExtension(Type type)
        {
            Type = type;
        }


        /// <summary>
        /// Provides value to control.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            string[] names = Enum.GetNames(Type);
            string[] values = new string[names.Length];

            for (int i = 0; i < names.Length; i++)
                values[i] = Resources.ResourceManager.GetString(names[i]);

            return values;
        }
    }
}
