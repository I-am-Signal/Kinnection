using System.ComponentModel.DataAnnotations.Schema;

namespace Kinnection
{
    public class User : BaseMixin
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public required DateTime Created { get; set; } = DateTime.UtcNow;
        public required string Fname { get; set; }
        public required string Lname { get; set; }
        public required string Email { get; set; }
        public required bool GoogleSO { get; set; }
    }

    public class Password : BaseMixin
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public required DateTime Created { get; set; } = DateTime.UtcNow;
        public required User User { get; set; }
        public required string PassString { get; set; }
    }

    public class Authentication : BaseMixin
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public required DateTime Created { get; set; } = DateTime.UtcNow;
        public required User User { get; set; }
        public required string Authorization { get; set; }
        public required string Refresh { get; set; }
        public required string PrevRef { get; set; }
    }
}