using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Ura.Models;

namespace Ura.Data.Mappings
{
    public class AbilityMap : ClassMapping<Ability>
    {
        public AbilityMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.Native);
            });

            Property(x => x.Deprecated);
            Property(x => x.Description);
        }
    }
}