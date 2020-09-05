using System.Windows;
using Ura.ViewModels.Screens;
using Ura.ViewModels;
using System.Diagnostics;

namespace Ura.Windows
{
    /// <summary>
    /// Interaction logic for Window.xaml
    /// </summary>
    public partial class ShellWindow : Window
    {
        UraSettingWindow win;

        public ShellWindow()
        {
            InitializeComponent();
            TraceListener debugListener = new MyTraceListener(Log);
            Debug.Listeners.Add(debugListener);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = new ShellWindowViewModel();

            var vm = (DataContext as ShellWindowViewModel);
            vm.PropertyChanged += (s, e1) =>
            {
                if (e1.PropertyName == "CurrentScreen")
                {
                    if (win == null || !win.IsActive)
                        OpenWindow(vm.CurrentScreen);
                }
            };

            //   vm.OpenRoles();
        }

        private void OpenWindow(ViewModelBase screen)
        {
            if (win != null)
            {
                win.Dispose();
            }
            win = new UraSettingWindow()
            {
                Owner = this,
                DataContext = screen
            };
            win.ShowDialog();
        }

        private void Log_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Log.ScrollToEnd();
        }
    }
}