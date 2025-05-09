namespace E_Commerce.Models.Entities
{
    public class Refreshtoken
    {
        public required Guid UserId { get; set; }
        public required string RefreshToken {  get; set; }
        public DateTime? ExpiryTime { get; set; } = DateTime.UtcNow.AddDays(7);
        public required string DeviceId { get; set; } 
        public string? UserAgent { get; set; }
    }
}
