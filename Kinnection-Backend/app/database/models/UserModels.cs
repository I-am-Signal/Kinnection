namespace Kinnection
{
    public class User : BaseMixin
    {
        public required int ID { get; set; }
        public required DateTime Created { get; set; } = DateTime.UtcNow;
        public required string Fname { get; set; }
        public required string Lname { get; set; }
        public required string Email { get; set; }
        public required string Token { get; set; }
        public required bool GoogleSO { get; set; }
    }

    public class Password : BaseMixin
    {
        public required int ID { get; set; }
        public required DateTime Created { get; set; } = DateTime.UtcNow;
        public required User User { get; set; }
        public required string PassString { get; set; }
    }
}