using IMK_web.Models.ModelHelper;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace IMK_web.Services
{
    public class IMKHelperService : IIMKHelperService
    {
        private static string _azureMapKey;
        private static HttpClient _client;
        private static string _azureMapURL;
        public IMKHelperService(IOptions<AppSettings> appSettings)
        {
            _azureMapKey = appSettings.Value.AzureMapsKey;
            _azureMapURL = appSettings.Value.AzureMapsURL;
            _client = new HttpClient();
        }
        public async Task<AzureCountryResultModel> geCountryFromAzureMaps(string latitude, string longtiude)
        {
            var azureMapAddress = _azureMapURL;
            var uriBuilder = new UriBuilder(azureMapAddress);

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["subscription-key"] = _azureMapKey;
            query["api-version"] = "1.0";
            if (String.IsNullOrEmpty(latitude) || String.IsNullOrEmpty(longtiude))
            {
                return null;
            }
            query["query"] = latitude + ',' + longtiude;

            uriBuilder.Query = query.ToString();
            azureMapAddress = uriBuilder.ToString();

            var response = await _client.GetAsync(azureMapAddress);
            var data = await response.Content.ReadAsStringAsync();

            JObject json = JObject.Parse(data);

            var addressesArray = json.GetValue("addresses")[0];
            var firstAddress = JObject.Parse(addressesArray.ToString());
            var countryInfo = new AzureCountryResultModel()
            {
                CountryName = JObject.Parse(firstAddress.GetValue("address").ToString()).GetValue("country").ToString(),
                CountryCode = JObject.Parse(firstAddress.GetValue("address").ToString()).GetValue("countryCode").ToString()
            };

            return countryInfo;
        }

    }
}
