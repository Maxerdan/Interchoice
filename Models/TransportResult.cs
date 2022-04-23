namespace Interchoice.Models
{
    public class TransportResult
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
        public object Value { get; set; }

        public TransportResult(int statusCode, string message, bool success, object value = null)
        {
            StatusCode= statusCode;
            Message= message;
            IsSuccess= success;
            Value= value;
        }
    }
}
