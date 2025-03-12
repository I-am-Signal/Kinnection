using System.ComponentModel.DataAnnotations.Schema;

namespace Kinnection
{
    public class Encryption : BaseMixin
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public required DateTime Created { get; set; } = DateTime.UtcNow;
        public required string Public { get; set; }
        public required string Private { get; set; }
    }
}