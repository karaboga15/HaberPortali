namespace HaberPortali.API.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int NewsId { get; set; }
        public News News { get; set; } = null!;
        public string AppUserId { get; set; } = null!;
        public AppUser AppUser { get; set; } = null!;
    }
}