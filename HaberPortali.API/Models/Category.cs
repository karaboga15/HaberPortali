namespace HaberPortali.API.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<News> News { get; set; } = new List<News>();
    }
}