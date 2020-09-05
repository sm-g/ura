namespace Ura.Data.Mappings
{
    // No Dictionaty support for OverAbilities Mapping, use xml mapping
    //public class UserMap : ClassMapping<User>
    //{
    //    public UserMap()
    //    {
    //        Table("User_");
    //        Id(x => x.Id, m =>
    //        {
    //            m.Generator(Generators.Native);
    //        });

    //        Property(x => x.Deprecated);
    //        Property(x => x.Login);
    //        Property(x => x.Password, m => m.Column("Pass"));

    //        Set(x => x.Roles, s =>
    //        {
    //            s.Table("UsersRoles");
    //            s.Key(k =>
    //            {
    //                k.Column("UserID");
    //            });
    //            s.Cascade(Cascade.All);
    //            s.Access(Accessor.Field);
    //        }, r =>
    //        {
    //            r.ManyToMany(x =>
    //            {
    //                x.Column("RoleID");
    //                x.Class(typeof(Role));
    //            });
    //        });
    //        Map(x => x.OverAbilities, m =>
    //            {
    //                m.Key(k => k.Column("UserID"));
    //                m.Table("UsersAbilities");
    //                m.Access(Accessor.Field);
    //            },
    //            k => k.ManyToMany(m =>
    //            {
    //                m.Column("AbilityID");
    //            }),
    //            v => v.Element(m =>
    //            {
    //                m.Column("ToAdd");
    //            })
    //        );
    //    }
    //}
}