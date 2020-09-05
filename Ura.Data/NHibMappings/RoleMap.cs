using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Ura.Models;

namespace Ura.Data.Mappings
{
    public class RoleMap : ClassMapping<Role>
    {
        public RoleMap()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.Native);
            });

            Property(x => x.Deprecated);
            Property(x => x.Description);
            Set(x => x.Abilities, s =>
            {
                s.Table("RolesAbilities");
                s.Key(k =>
                {
                    k.Column("RoleID");
                });
                s.Access(Accessor.Field);
            }, r =>
            {
                r.ManyToMany(x =>
                {
                    x.Column("AbilityID");
                    x.Class(typeof(Ability));
                });
            });
        }
    }
}