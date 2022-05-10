namespace Interchoice.Models
{
    public class TransportResult
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public object Value { get; set; }

        public TransportResult(int statusCode, string message, object value = null)
        {
            StatusCode= statusCode;
            Message= message;
            Value= value;
        }
    }
}
