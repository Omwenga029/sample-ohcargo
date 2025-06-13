namespace backend.Models
{
    public class User
    {
        public int Id { get; set; }  // Primary key
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // (You’ll hash this later)
    }
}
