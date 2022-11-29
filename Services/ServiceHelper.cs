using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IMK_web.Services
{
    public static class ServiceHelper
    {
        public static T GetResult<T>(RestClient client, RestRequest request)
        {
            var response = client.Execute(request);
            return JsonConvert.DeserializeObject<T>(response.Content, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }
    }
}
