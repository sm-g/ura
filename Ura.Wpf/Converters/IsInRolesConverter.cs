using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Ura.ViewModels;

namespace Ura
{
    public class IsInRolesConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
                object parameter, CultureInfo culture)
        {
            if (values[0] is RoleVM && values[1] is IEnumerable<UserVM>)
            {
                RoleVM item = values[0] as RoleVM;
                var collection = values[1] as IEnumerable<UserVM>;

                if (collection.Count() > 0)
                {
                    //if (collection.All(user => user.Roles.Contains(a)))
                    //    return true;

                    //if (collection.Any(user => user.Roles.Contains(a)))
                    //    return null;
                }
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}