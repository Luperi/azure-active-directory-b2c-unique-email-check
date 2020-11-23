namespace MyProject.Function
{
    public class ResponseContent
    {
        private const string APIVERSION = "1.0.0";

        public ResponseContent()
        {
            this.Version = ResponseContent.APIVERSION;
            this.Action = "Continue";
        }

        public ResponseContent(string action = "Continue", string userMessage = "", string status = "200")
        {
            this.Version = ResponseContent.APIVERSION;
            this.Action = action;
            this.UserMessage = userMessage;
            this.Status = status;
        }

        public string Version { get; }
        public string Action { get; set; }
        public string UserMessage { get; set; }
        public string Status { get; set; }

    }
}