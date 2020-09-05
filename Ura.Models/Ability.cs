namespace Ura.Models
{
    public class Ability : IDeletable
    {
        public virtual int Id { get; set; }
        public virtual string Description { get; set; }
        public virtual bool Deprecated { get; set; }

        public override string ToString()
        {
            return Description;
        }
    }
}