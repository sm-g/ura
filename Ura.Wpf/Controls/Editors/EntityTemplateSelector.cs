using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Ura.ViewModels;

namespace Ura.Controls.Editors
{
    public class EntityTemplateSelector : DataTemplateSelector
    {
        public DataTemplate UserTemplate { get; set; }
        public DataTemplate RoleTemplate { get; set; }
        public DataTemplate AbilityTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null)
            {
                if (item is UserVM)
                {
                    return UserTemplate;
                }
                if (item is RoleVM)
                {
                    return RoleTemplate;
                }
                if (item is AbilityVM)
                {
                    return AbilityTemplate;
                }
            }

            return null;
        }
    }
}
