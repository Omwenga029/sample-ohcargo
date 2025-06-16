using Microsoft.AspNetCore.Http;

namespace backend.Models.DTOs
{
    public class CommunityThreadDto
    {
        public string? Content { get; set; }
        public IFormFile? Image { get; set; }
        public IFormFile? Video { get; set; }
    }
}
