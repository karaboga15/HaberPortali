using System;

namespace HaberPortali.API.DTOs
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime CreatedDate { get; set; }

        public int NewsId { get; set; }
        public string UserName { get; set; }
    }
}