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
using System.Windows.Shapes;
using EventAggregator;
using Ura.ViewModels.Screens;
using Ura.Data;
using Ura.ViewModels;

namespace Ura.Windows
{
    /// <summary>
    /// Interaction logic for UraSettingWindow.xaml
    /// </summary>
    public partial class UraSettingWindow : Window, IDisposable
    {
        EventMessageHandler handler;
        public UraSettingWindow()
        {
            InitializeComponent();
            handler = this.Subscribe((int)Event.OpenSelector, (e) =>
            {
                var vm = e.GetValue<SelectorViewModel>(Messenger.Selector);
                var dialog = new SelectorDialog();
                dialog.Owner = this;
                dialog.DataContext = vm;
                dialog.ShowDialog();
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var vm = DataContext as IDialog;
            if (vm.DialogResult == null && vm.CanApply)
            {
                switch (MessageBox.Show(this, "Сохранить изменения?", vm.Title, MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation, MessageBoxResult.Cancel))
                {
                    case MessageBoxResult.Yes:
                        vm.OkCommand.Execute(null);
                        break;
                    case MessageBoxResult.No:
                        vm.CancelCommand.Execute(null);
                        break;
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
        }

        public void Dispose()
        {
            handler.Dispose();
        }
    }
}
