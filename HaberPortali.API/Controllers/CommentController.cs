using HaberPortali.API.DTOs;
using HaberPortali.API.Models;
using HaberPortali.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HaberPortali.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly IGenericRepository<Comment> _repository;

        public CommentController(IGenericRepository<Comment> repository)
        {
            _repository = repository;
        }

        [HttpGet("news/{newsId}")]
        public async Task<IActionResult> GetByNewsId(int newsId)
        {
            var allComments = await _repository.GetAllAsync();
            var newsComments = allComments.Where(c => c.NewsId == newsId)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    Text = c.Text,
                    CreatedDate = c.CreatedDate,
                    NewsId = c.NewsId
                }).ToList();

            return Ok(newsComments);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Add(CommentDto commentDto)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var comment = new Comment
            {
                Text = commentDto.Text,
                NewsId = commentDto.NewsId,
                AppUserId = userId,
                CreatedDate = DateTime.Now
            };

            await _repository.AddAsync(comment);
            return Ok("Yorum başarıyla eklendi.");
        }
    }
}