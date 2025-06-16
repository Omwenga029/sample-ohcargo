using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<CommunityThread> Threads { get; set; } = new List<CommunityThread>();
        public bool IsMember { get; set; } = true; // All registered users are considered members
    }
}
