namespace E_Commerce.DTO.LoggingDtos
{
    public class RequestDto
    {
        public string Timestamp { get; set; }
        public string IpAddress { get; set; }
        public string Method { get; set; }
        public string Url { get; set; }
        public string User { get; set; }
        public string Headers { get; set; }
        public string Body { get; set; }
    }
}
