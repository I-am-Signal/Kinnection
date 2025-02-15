namespace Kinnection
{
    public class OkResponse
    {
        public string message { get; set; }
    }

    public class UserResponse
    {
        public int ID { get; set; }
        public string Fname { get; set; }
        public string Lname { get; set; }
        public string Email { get; set; }
    }
}