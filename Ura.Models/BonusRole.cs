using System.Collections.Generic;

namespace Ura.Models
{
    public class BonusRole : Role
    {
        public override bool Deprecated
        {
            get
            {
                return false;
            }
        }
        public BonusRole(IEnumerable<Ability> ablilties)
            : base(ablilties)
        {
            Description = "bonus";
        }
    }
}