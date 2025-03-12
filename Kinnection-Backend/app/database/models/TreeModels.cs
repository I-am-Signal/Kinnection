using System.ComponentModel.DataAnnotations.Schema;

namespace Kinnection
{
    public class Tree : BaseMixin
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public required DateTime Created { get; set; } = DateTime.UtcNow;
        public required User User { get; set; }
        public required string Name { get; set; }
        public required int SelfID { get; set; }
    }

    public class Member : BaseMixin
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public required DateTime Created { get; set; } = DateTime.UtcNow;
        public virtual required Tree Tree { get; set; }
        public required string Fname { get; set; }
        public required string? Mnames { get; set; }
        public required string Lname { get; set; }
        public required bool Sex { get; set; }
        public required DateOnly DOB { get; set; }
        public required string? Birthplace { get; set; }
        public required DateOnly DOD { get; set; }
        public required string? Deathplace { get; set; }
        public required string? CauseOfDeath { get; set; }
        public required string? Ethnicity { get; set; }
        public required string? Biography { get; set; }
    }
}