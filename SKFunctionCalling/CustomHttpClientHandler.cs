using System.Net.Http.Headers;

namespace SKFunctionCalling
{
    public class CustomHttpClientHandler : HttpClientHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine("REQUEST HANDLER");
            string baseUrl = request.RequestUri.GetLeftPart(UriPartial.Authority);
            string url = request.RequestUri.ToString(); ;
            HttpMethod method = request.Method;
            string queryString = request.RequestUri.Query;
            string requestBody = null;
            if (request.Content != null)
            {
                requestBody = await request.Content.ReadAsStringAsync();
            }
            Console.WriteLine("    URL: " + url);
            Console.WriteLine("    Method: " + method);
            Console.WriteLine("    Query string: " + queryString);
            Console.WriteLine("    Request body: " + requestBody);

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var content = await response.Content.ReadAsStringAsync();
            return response;
        }
    }
}
