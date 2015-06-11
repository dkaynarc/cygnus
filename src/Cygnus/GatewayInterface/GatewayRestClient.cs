using RestSharp;
using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Cygnus.GatewayInterface
{
    public class CygnusApiProxy
    {
        const string BaseUrl = "https://localhost:44300/api";
        
        public T Execute<T>(RestRequest request) where T : new()
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(BaseUrl);
            var response = client.Execute<T>(request);
            
            if (response.ErrorException != null)
            {
                const string message = "Error retrieving response. Check inner details for more info.";
                var myException = new ApplicationException(message, response.ErrorException);
                throw myException;
            }

            return response.Data;
        }

        public Gateway PostGateway(Gateway gateway)
        {
            var request = new RestRequest(Method.POST);
            request.JsonSerializer = new JsonSerializer();
            request.AddJsonBody(gateway);
            request.Resource = "Gateways";
            return Execute<Gateway>(request);
        }

        public Resource PostResource(Resource resource)
        {
            var request = new RestRequest(Method.POST);
            request.JsonSerializer = new JsonSerializer();
            request.AddJsonBody(resource);
            request.Resource = "Resources";
            return Execute<Resource>(request);
        }

        public List<Resource> GetResources()
        {
            var request = new RestRequest(Method.GET);
            request.Resource = "Resources";
            return Execute<List<Resource>>(request);
        }

        public Resource DeleteResource(Resource resource)
        {
            var request = new RestRequest(Method.DELETE);
            request.Resource = "Resources/{ResourceId}";
            request.AddParameter("ResourceId", resource.Id, ParameterType.UrlSegment);
            return Execute<Resource>(request);
        }
    }

    public class Resource
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Uri { get; set; }
        public Guid GatewayId { get; set; }
    }

    public class Gateway
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
