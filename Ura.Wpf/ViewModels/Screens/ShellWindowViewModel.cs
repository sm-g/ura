using System.Windows.Input;

namespace Ura.ViewModels.Screens
{
    public class ShellWindowViewModel : ViewModelBase
    {
        private ViewModelBase _currentScreen;
        private ICommand _setScreen;

        public ViewModelBase CurrentScreen
        {
            get
            {
                return _currentScreen;
            }
            set
            {
                if (_currentScreen != value)
                {
                    if (_currentScreen != null)
                        _currentScreen.Dispose();
                    _currentScreen = value;
                    OnPropertyChanged(() => CurrentScreen);
                }
            }
        }
        public ICommand SetScreenCommand
        {
            get
            {
                return _setScreen
                   ?? (_setScreen = new RelayCommand<string>((name) =>
                        {
                            switch (name)
                            {
                                case "user": CurrentScreen = new UserScreenViewModel(); break;
                                case "role": CurrentScreen = new RoleScreenViewModel(); break;
                                case "ability": CurrentScreen = new AbilityScreenViewModel(); break;
                            }
                        }));
            }
        }

        public ShellWindowViewModel()
        {
        }

        internal void OpenUsers()
        {
            CurrentScreen = new UserScreenViewModel();
        }
        internal void OpenRoles()
        {
            CurrentScreen = new RoleScreenViewModel();
        }
        internal void OpenAbilities()
        {
            CurrentScreen = new AbilityScreenViewModel();
        }
    }
}