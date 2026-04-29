namespace BarClipApi.Models.Dto
{
    public class SessionDto
    {
        public required Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? Title { get; set; }
        public string? ThumbnailUrl { get; set; }
    }
}
