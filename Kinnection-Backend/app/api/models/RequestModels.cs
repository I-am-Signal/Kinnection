namespace Kinnection
{
    public class UserRequest
    {
        public string Fname { get; set; } = string.Empty;
        public string Lname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class PostUserRequest : UserRequest
    {
        public string Password { get; set; } = string.Empty;
    }
}