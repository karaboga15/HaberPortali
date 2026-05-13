using HaberPortali.API.DTOs;
using HaberPortali.API.Models;
using HaberPortali.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HaberPortali.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly IGenericRepository<News> _repository;
        public NewsController(IGenericRepository<News> repository) => _repository = repository;

        [HttpGet]
        public async Task<IActionResult> GetAll(string? search = null)
        {
            var list = await _repository.GetAllAsync();
            if (!string.IsNullOrEmpty(search))
                list = list.Where(n => n.Title.ToLower().Contains(search.ToLower())).ToList();
            var result = list.Select(n => new NewsDto
            {
                Id = n.Id,
                Title = n.Title,
                Content = n.Content,
                ImagePath = n.ImagePath,
                CreatedDate = n.CreatedDate,
                ViewCount = n.ViewCount,
                CategoryId = n.CategoryId
            });
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var n = await _repository.GetByIdAsync(id);
            if (n == null) return NotFound();
            n.ViewCount++;
            _repository.Update(n);
            return Ok(new NewsDto
            {
                Id = n.Id,
                Title = n.Title,
                Content = n.Content,
                ImagePath = n.ImagePath,
                CreatedDate = n.CreatedDate,
                ViewCount = n.ViewCount,
                CategoryId = n.CategoryId
            });
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var list = await _repository.GetAllAsync();
            var result = list.Where(n => n.CategoryId == categoryId).Select(n => new NewsDto
            {
                Id = n.Id,
                Title = n.Title,
                Content = n.Content,
                ImagePath = n.ImagePath,
                CreatedDate = n.CreatedDate,
                ViewCount = n.ViewCount,
                CategoryId = n.CategoryId
            });
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Add(NewsDto dto)
        {
            var news = new News
            {
                Title = dto.Title,
                Content = dto.Content,
                ImagePath = dto.ImagePath,
                CategoryId = dto.CategoryId,
                CreatedDate = DateTime.Now
            };
            await _repository.AddAsync(news);
            return Ok(new ResultDto { Status = true, Message = "Haber eklendi." });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> Update(NewsDto dto)
        {
            var news = await _repository.GetByIdAsync(dto.Id);
            if (news == null) return NotFound(new ResultDto { Status = false, Message = "Haber bulunamadı." });
            news.Title = dto.Title; news.Content = dto.Content;
            news.ImagePath = dto.ImagePath; news.CategoryId = dto.CategoryId;
            _repository.Update(news);
            return Ok(new ResultDto { Status = true, Message = "Haber güncellendi." });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var news = await _repository.GetByIdAsync(id);
            if (news == null) return NotFound(new ResultDto { Status = false, Message = "Haber bulunamadı." });
            _repository.Remove(news);
            return Ok(new ResultDto { Status = true, Message = "Haber silindi." });
        }
    }
}
