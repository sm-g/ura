using System;
using Ura.Models;

namespace Ura.ViewModels
{
    public class RoleVM : EntityBaseVM
    {
        internal readonly Role role;
        public int Id { get { return role.Id; } }

        public string Description
        {
            get
            {
                return role.Description;
            }
            set
            {
                if (role.Description != value)
                {
                    role.Description = value;
                    OnPropertyChanged(() => Description);
                    OnPropertyChanged(() => Represent);
                }
            }
        }
        public override bool Deprecated
        {
            get { return role.Deprecated; }
            set
            {
                if (role.Deprecated != value)
                {
                    OnDeprChanged(value);
                    OnPropertyChanged("Deprecated");
                }
            }
        }
        public override string Represent
        {
            get { return Description; }
        }

        public override bool IsUnsaved
        {
            get { return Id == 0; }
        }

        public RoleVM(Role role)
        {
            if (role == null)
                throw new ArgumentNullException("role");
            this.role = role;
        }
        public override bool Filter(string query)
        {
            return Description.ToLower().StartsWith(query.ToLower());
        }

        public override string ToString()
        {
            return Description;
        }
    }
}