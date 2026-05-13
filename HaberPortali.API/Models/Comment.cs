using System;

namespace HaberPortali.API.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int NewsId { get; set; }
        public News News { get; set; }

        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}