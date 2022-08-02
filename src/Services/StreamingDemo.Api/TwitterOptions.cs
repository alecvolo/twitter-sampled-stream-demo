namespace StreamingDemo.Api
{
    public class TwitterOptions
    {
        public TwitterOptions()
        {
        }
        public TwitterOptions(string bearerToken, string uri)
        {
            this.Uri = uri;
            this.BearerToken = bearerToken;
        }
        public string BearerToken { get; set; } = null!;

        public string Uri { get; set; } = null!;
    }

}
