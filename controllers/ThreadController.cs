using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using backend.Data;
using backend.Models;
using backend.Models.DTOs;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ThreadsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContext;

        public ThreadsController(AppDbContext context, IWebHostEnvironment env, IHttpContextAccessor httpContext)
        {
            _context = context;
            _env = env;
            _httpContext = httpContext;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateThread([FromForm] CommunityThreadDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized("User not found");

            int userId = int.Parse(userIdClaim.Value);

            string? imageUrl = null;
            string? videoUrl = null;

            if (dto.Image != null)
            {
                var imagePath = Path.Combine("uploads", Guid.NewGuid().ToString() + Path.GetExtension(dto.Image.FileName));
                var fullImagePath = Path.Combine(_env.WebRootPath, imagePath);
                Directory.CreateDirectory(Path.GetDirectoryName(fullImagePath)!);
                using (var stream = new FileStream(fullImagePath, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream);
                }
                imageUrl = "/" + imagePath.Replace("\\", "/");
            }

            if (dto.Video != null)
            {
                var videoPath = Path.Combine("uploads", Guid.NewGuid().ToString() + Path.GetExtension(dto.Video.FileName));
                var fullVideoPath = Path.Combine(_env.WebRootPath, videoPath);
                Directory.CreateDirectory(Path.GetDirectoryName(fullVideoPath)!);
                using (var stream = new FileStream(fullVideoPath, FileMode.Create))
                {
                    await dto.Video.CopyToAsync(stream);
                }
                videoUrl = "/" + videoPath.Replace("\\", "/");
            }

            var thread = new CommunityThread
            {
                Content = dto.Content,
                ImageUrl = imageUrl,
                VideoUrl = videoUrl,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.CommunityThreads.Add(thread);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                thread.Id,
                thread.Content,
                thread.ImageUrl,
                thread.VideoUrl,
                thread.CreatedAt
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllThreads()
        {
            var threads = await _context.CommunityThreads
                .Include(t => t.User)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(threads.Select(t => new
            {
                t.Id,
                t.Content,
                t.ImageUrl,
                t.VideoUrl,
                t.CreatedAt,
                User = new
                {
                    t.User!.Id,
                    t.User.Username
                }
            }));
        }
    }
}
