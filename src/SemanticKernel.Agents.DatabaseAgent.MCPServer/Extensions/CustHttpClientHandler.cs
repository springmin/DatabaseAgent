using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticKernel.Agents.DatabaseAgent.MCPServer.Extensions
{
    internal class CustHttpClientHandler : HttpClientHandler
    {
        private string llmService=string.Empty;
        private string url = string.Empty;

        public CustHttpClientHandler(string LlmService,string endpoint):base() 
        {
            this.llmService= LlmService;
            this.url = endpoint;

        }
   
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            UriBuilder uriBuilder;
            Uri uri = new Uri(url);
            string host = uri.Host;
            switch (request.RequestUri?.LocalPath)
            {
                case "/v1/chat/completions":
                    switch (llmService)
                    {
                        case "CustOpenAI":
                            uriBuilder = new UriBuilder(request.RequestUri)
                            {
                                /*change your need  parameter*/
                                //Scheme = uri.Scheme,
                                //Host = uri.Host,
                                //Path = "api/paas/v4/chat/completions",
                            };
                            //request.RequestUri = uriBuilder.Uri;
                            break;
                        default:
                            
                            break;
                    }
                    break;
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            return response;
        }
    }
}
