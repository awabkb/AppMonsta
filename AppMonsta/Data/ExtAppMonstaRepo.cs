using AppMonsta.Models;
using Newtonsoft.Json;
using RestSharp;
using System.Text;
using System.Text.RegularExpressions;

namespace AppMonsta.Data
{
    public class ExtAppMonstaRepo : IExtAppMonstaRepo

    {
        private readonly IConfiguration _configuration;
        private readonly string baseUrl;
        private readonly string username;
        private readonly string password;

        public ExtAppMonstaRepo(IConfiguration configuration)
        {
            _configuration = configuration;
            // Get the values from the configuration
            username = _configuration.GetValue<string>("AppMonstaApi:Username");
            password = _configuration.GetValue<string>("AppMonstaApi:Password");
            baseUrl = _configuration.GetValue<string>("AppMonstaApi:BaseUrl");
        }

        public async Task<List<AggregatedRanking>> GetAggregatedRankings(string store, string date, string countryCode)
        {

            // Build the request URL
            string requestUrl = $"{baseUrl}{store}/rankings/aggregate.json";

            // Create a RestClient
            var client = new RestClient(requestUrl);

            // Create a RestRequest
            var request = new RestRequest(Method.Get.ToString());

            // Set the basic auth credentials
            request.AddHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password)));

            // Add the query parameters
            request.AddQueryParameter("country", countryCode);
            request.AddQueryParameter("date", date);

            // Set the Accept-Encoding header to turn on compression
            request.AddHeader("Accept-Encoding", "deflate, gzip");

            // Execute the request asynchronously and get the response
            RestResponse response = await client.ExecuteAsync(request);
            if (response.IsSuccessful) {
                // Parse the response
                var aggregatedRankings = new List<AggregatedRanking>();
                // Wrap the JSON response in square brackets and add a dot between curly brackets, ignoring any spaces between the brackets
                string correctedjson = "[" + Regex.Replace(response.Content, @"\}\s*\{", "},{") + "]";

                dynamic aggregatedRankingsJson = JsonConvert.DeserializeObject<List<AggregatedRanking>>(correctedjson);
                //max 3 iterarions for testing
                int i = 0;
                foreach (var aggregatedRanking in aggregatedRankingsJson)
                {
                    if (i++ >= 3) break;
                    aggregatedRankings.Add(new AggregatedRanking()
                    {
                        Country = aggregatedRanking.Country,
                        Genre_id = aggregatedRanking.Genre_id,
                        Rank_id = aggregatedRanking.Rank_id,
                        Ranks = aggregatedRanking.Ranks
                    });
                }
                return aggregatedRankings;

            }
            throw new Exception(response.Content);


        }

        public async Task<List<AppDetails>> GetAllAppsDetails(string store, string date, string countryCode,string genreId)
        {
            // Build the request URL
            string requestUrl = $"{baseUrl}{store}/details.json";

            // Create a RestClient
            var client = new RestClient(requestUrl);

            // Create a RestRequest
            var request = new RestRequest(Method.Get.ToString());

            // Set the basic auth credentials
            request.AddHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password)));

            // Add the query parameters
            request.AddQueryParameter("country", countryCode);
            request.AddQueryParameter("date", date);

            // Set the Accept-Encoding header to turn on compression
            request.AddHeader("Accept-Encoding", "deflate, gzip");

            // Execute the request asynchronously and get the response
            RestResponse response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                // Wrap the JSON response in square brackets and add a dot between curly brackets, ignoring any spaces between the brackets
                string correctedjson = "[" + Regex.Replace(response.Content, @"\}\s*\{", "},{") + "]";

                var appDetailsList = JsonConvert.DeserializeObject<List<AppDetails>>(correctedjson);
                return appDetailsList.Where(x=>x.genre_ids.Contains(genreId)).ToList();

            }
            throw new Exception(response.Content);
        }

        public async Task<AppDetails> GetAppDetails(string store, string countryCode, string appId)
        {
            // Build the request URL
            string requestUrl = $"{baseUrl}{store}/details/{appId}.json";

            // Create a RestClient
            var client = new RestClient(requestUrl);

            // Create a RestRequest
            var request = new RestRequest(Method.Get.ToString());

            // Set the basic auth credentials
            request.AddHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password)));

            // Add the query parameters
            request.AddQueryParameter("country", countryCode);

            // Set the Accept-Encoding header to turn on compression
            request.AddHeader("Accept-Encoding", "deflate, gzip");

            // Execute the request asynchronously and get the response
            RestResponse response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {

                // Parse the response
                var appDetails = JsonConvert.DeserializeObject<AppDetails>(response.Content);
                return appDetails;
            }
            throw new Exception(response.Content);
        }

        public async Task<List<Genre>> GetGenresRanking(string store, string date, string countryCode)
        {
            // Request URL
            string requestUrl = $"{baseUrl}{store}/rankings/genres.json";

            // Create a RestClient
            var client = new RestClient(requestUrl);

            // Create a RestRequest
            var request = new RestRequest(Method.Get.ToString());

            // Set the basic auth credentials
            request.AddHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password)));
            
            // Add the query parameters
            request.AddQueryParameter("date", date);

            // Set the Accept-Encoding header to turn on compression
            request.AddHeader("Accept-Encoding", "deflate, gzip");

            // Execute the request asynchronously and get the response
            RestResponse response = await client.ExecuteAsync(request);

            // Parse the response
            var genres = new List<Genre>();
            using (var stream = new MemoryStream(response.RawBytes))
            using (var reader = new StreamReader(stream))
            {
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    // Parse the JSON response to get the Genre_id and Name values
                    dynamic json = JsonConvert.DeserializeObject(line);
                    var genre = new Genre
                    {
                        Genre_id = json.genre_id,
                        Name = json.name
                    };
                    genres.Add(genre);
                }
            }

            return genres;
        }
    }
}
