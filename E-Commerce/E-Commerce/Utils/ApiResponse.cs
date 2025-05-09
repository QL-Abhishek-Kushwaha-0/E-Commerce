namespace E_Commerce.Utils
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public object? Data { get; set; }
        public object? Error { get; set; }

        public ApiResponse(bool Success, int StatusCode, string Message, object? Data = null, object? Error = null)
        {
            this.Success = Success;
            this.StatusCode = StatusCode;
            this.Message = Message;
            this.Data = Data ?? new object();
            this.Error = Error ?? new object();
        }
    }
}
