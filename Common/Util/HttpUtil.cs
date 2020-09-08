using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Common.Util
{
    public class HttpUtil
    {
       public HttpClient Client { get; }

        private static IHttpClientFactory _clientFactory;

        public bool GetBranchesError { get; private set; }

        public HttpUtil(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public static async Task<string> OnGet()
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                "https://api.github.com/repos/aspnet/AspNetCore.Docs/branches");
            request.Headers.Add("Accept", "application/vnd.github.v3+json");
            request.Headers.Add("User-Agent", "HttpClientFactory-Sample");

            var client = _clientFactory.CreateClient();

            var response = await client.SendAsync(request);

                return await response.Content
                    .ReadAsStringAsync();
        }

    }
}
