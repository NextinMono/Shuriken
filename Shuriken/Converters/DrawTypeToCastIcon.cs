using System.Windows.Data;
using System.Windows.Markup;
using System;
using Shuriken.Models;

namespace Shuriken.Converters
{
    public class DrawTypeToCastIcon : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DrawType type = (DrawType)value;
            switch(type)
            {
                case DrawType.None:
                    return "\uf0c8";
                case DrawType.Sprite:
                    return "\uf03e";

                case DrawType.Font:
                    return "\uf031";
            }
            return "\uf02b";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}