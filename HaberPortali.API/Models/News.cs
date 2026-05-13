namespace HaberPortali.API.Models
{
    public class News
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string ImagePath { get; set; } = null!;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int ViewCount { get; set; } = 0;
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}