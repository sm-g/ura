using System.Windows.Input;

namespace Ura.ViewModels
{
    public interface IDialog
    {
        bool? DialogResult { get; }
        string Title { get; set; }
        ICommand OkCommand { get; }
        ICommand CancelCommand { get; }
        bool CanOk { get; }
        bool CanApply { get; }
    }
}