using System.Collections.Generic;

namespace Ura.Models
{
    public class Role : IDeletable
    {
        ISet<Ability> _abilities = new HashSet<Ability>();
        public virtual int Id { get; set; }
        public virtual string Description { get; set; }
        public virtual bool Deprecated { get; set; }
        public virtual ISet<Ability> Abilities { get { return _abilities; } }

        public Role()
        {
        }
        public Role(IEnumerable<Ability> ablilties)
        {
            _abilities = new HashSet<Ability>(ablilties);
        }

        public override string ToString()
        {
            return string.Format("{0} ({1}a)", Description, Abilities.Count);
        }
    }
}