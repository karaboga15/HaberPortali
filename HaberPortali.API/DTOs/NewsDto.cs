using System;
using System.ComponentModel.DataAnnotations;

namespace HaberPortali.API.DTOs
{
    public class NewsDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Haber başlığı zorunludur.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Haber içeriği zorunludur.")]
        public string Content { get; set; }
        public string ImagePath { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ViewCount { get; set; }

        [Required]
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
}