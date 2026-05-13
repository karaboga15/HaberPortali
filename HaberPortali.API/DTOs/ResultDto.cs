namespace HaberPortali.API.DTOs
{
    public class ResultDto
    {
        public bool Status { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }
    }
}