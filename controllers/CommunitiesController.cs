using backend.Models.DTOs;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommunityController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CommunityController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ✅ GET: /api/community/members
        [HttpGet("members")]
        public async Task<IActionResult> GetAllMembers()
        {
            var users = await _context.Users
                .Select(u => new { u.Id, u.Username, u.Email })
                .ToListAsync();

            return Ok(users);
        }

        // ✅ POST: /api/community/threads (Only for registered users)
        [Authorize]
        [HttpPost("threads")]
        public async Task<IActionResult> CreateThread([FromForm] CommunityThreadDto dto)
        {
            var userIdClaim = User.FindFirst("sub")?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized("User not authenticated.");
            }

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Invalid user ID.");
            }

            string? imagePath = null;
            string? videoPath = null;

            // Handle image upload
            if (dto.Image != null && dto.Image.Length > 0)
            {
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "images");
                Directory.CreateDirectory(uploadsDir);

                var fileName = $"{Guid.NewGuid()}_{dto.Image.FileName}";
                var filePath = Path.Combine(uploadsDir, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await dto.Image.CopyToAsync(stream);

                imagePath = $"/uploads/images/{fileName}";
            }

            // Handle video upload
            if (dto.Video != null && dto.Video.Length > 0)
            {
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "videos");
                Directory.CreateDirectory(uploadsDir);

                var fileName = $"{Guid.NewGuid()}_{dto.Video.FileName}";
                var filePath = Path.Combine(uploadsDir, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await dto.Video.CopyToAsync(stream);

                videoPath = $"/uploads/videos/{fileName}";
            }

            var thread = new CommunityThread
            {
                UserId = userId,
                Content = dto.Content ?? string.Empty,
                ImageUrl = imagePath,
                VideoUrl = videoPath,
                CreatedAt = DateTime.UtcNow
            };

            _context.CommunityThreads.Add(thread);
            await _context.SaveChangesAsync();

            return Ok(thread);
        }
    }
}
