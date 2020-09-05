using System;
using Ura.Models;

namespace Ura.ViewModels
{
    public class AbilityVM : EntityBaseVM
    {
        internal readonly Ability ability;

        public int Id { get { return ability.Id; } }
        public string Description
        {
            get
            {
                return ability.Description;
            }
            set
            {
                if (ability.Description != value)
                {
                    ability.Description = value;
                    OnPropertyChanged(() => Description);
                    OnPropertyChanged(() => Represent);
                }
            }
        }
        public override bool Deprecated
        {
            get { return ability.Deprecated; }
            set
            {
                if (ability.Deprecated != value)
                {
                    OnDeprChanged(value);
                    OnPropertyChanged(() => Deprecated);
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

        public AbilityVM(Ability ability)
        {
            if (ability == null)
                throw new ArgumentNullException("ability");
            this.ability = ability;
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