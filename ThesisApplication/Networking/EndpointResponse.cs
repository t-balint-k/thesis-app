namespace ThesisApplication
{
    public class EndpointResponse
    {
        public bool success;
        public string message;

        public EndpointResponse(bool _success, string _message)
        {
            success = _success;
            message = _message;
        }
    }
}