using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ura.Controls
{
    /// <summary>
    /// Interaction logic for ListItem.xaml
    /// </summary>
    public partial class ChkListItem : UserControl
    {
        public ChkListItem()
        {
            InitializeComponent();
        }


        public bool HighlightUnchecked
        {
            get { return (bool)GetValue(HighlightUncheckedProperty); }
            set { SetValue(HighlightUncheckedProperty, value); }
        }

        public static readonly DependencyProperty HighlightUncheckedProperty =
            DependencyProperty.Register("HighlightUnchecked", typeof(bool), typeof(ChkListItem));


    }
}
