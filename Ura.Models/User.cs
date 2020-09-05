using System.Collections.Generic;

namespace Ura.Models
{
    public class User : IDeletable
    {
        private ISet<Role> _roles = new HashSet<Role>();
        private IDictionary<Ability, bool> _overAbilities = new Dictionary<Ability, bool>();

        public virtual int Id { get; set; }

        public virtual string Login { get; set; }

        public virtual string Password { get; set; }

        public virtual bool Deprecated { get; set; }

        public virtual ISet<Role> Roles { get { return _roles; } }

        public virtual IDictionary<Ability, bool> OverAbilities { get { return _overAbilities; } }

        public override string ToString()
        {
            return string.Format("{0} ({1}r {2}oa)", Login, Roles.Count, OverAbilities.Count);
        }
    }
}