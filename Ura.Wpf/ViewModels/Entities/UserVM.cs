using System;
using Ura.Models;

namespace Ura.ViewModels
{
    public class UserVM : EntityBaseVM
    {
        internal readonly User user;

        public int Id
        {
            get { return user.Id; }
        }

        public string Login
        {
            get
            {
                return user.Login;
            }
            set
            {
                if (user.Login == value)
                    return;
                user.Login = value;
                OnPropertyChanged(() => Login);
                OnPropertyChanged(() => Represent);
            }
        }

        public string Password
        {
            get
            {
                return user.Password;
            }
            set
            {
                if (user.Password != value)
                {
                    user.Password = value;
                    OnPropertyChanged(() => Password);
                }
            }
        }

        public override bool Deprecated
        {
            get { return user.Deprecated; }
            set
            {
                if (user.Deprecated != value)
                {
                    OnDeprChanged(value);
                    OnPropertyChanged("Deprecated");
                }
            }
        }

        public override string Represent
        {
            get { return Login; }
        }

        public override bool IsUnsaved
        {
            get { return Id == 0; }
        }

        public UserVM(User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            this.user = user;
        }

        public override bool Filter(string query)
        {
            return Login.ToLower().StartsWith(query.ToLower());
        }

        public override string ToString()
        {
            return Login;
        }
    }
}