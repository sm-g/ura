using System;
using System.Globalization;
using System.Windows.Data;

namespace Ura
{
    public class AbilIsCheckedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                object parameter, CultureInfo culture)
        {
            if ((value == null || value is bool) && parameter is string)
            {
                var column = int.Parse(parameter as string);
                if (column == 0)
                    return value == null;

                if ((value as bool?).HasValue)
                {
                    switch (column)
                    {
                        case 1: return (bool)value;
                        case 2: return !(bool)value;
                    }
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}