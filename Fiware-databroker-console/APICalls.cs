using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Databroker
{
    // This class encapsules all the calls to the different APIs. It uses base-URLs defined in appsettings.json
    public class APICalls
    {
        readonly string FiwareUrl; // Contains the URL to Fiware
        readonly string BrokerUrl; // Contains the URL to broker
        public APICalls()
        {
            FiwareUrl = "http://localhost:1026/"; // Set Fiware Url
            FiwareUrl = "http://192.168.5.146:1026/";
        }

        public string getFiwareUrl()
        {
            return FiwareUrl;
        }

        // Perform a request to an API by using GET
        public async Task<string> PerformGetRequest(string BaseUrl = "", string Endpoint = "")
        {
            // BaseUrl represents the general URl of the API
            // Endpoint sets the specific target

            Console.WriteLine("URL: " + BaseUrl + Endpoint);

            if (BaseUrl == null)
            {
                return "No base-URL set!";
            }

            var client = new HttpClient();
            var callResult = "";

            try {
                callResult = await client.GetStringAsync(BaseUrl + Endpoint);
            } 
            catch (Exception ex) 
            {
                Console.WriteLine("Error on getting data: " + ex.Message);
            }

            return callResult.Length > 0 ? callResult : "Error reading from " + BaseUrl + Endpoint; // Return result or error
        }


        // The following methods call specific parts of the APIs and should self-explanatory
        public string GetFiwareVersion()
        {
            return PerformGetRequest(FiwareUrl, "version").Result.ToString();           
        }

        public string GetEntryPoints()
        {
            return PerformGetRequest(FiwareUrl, "v2").Result.ToString();
        }

        public string GetEntities(string parameters = "")
        {
            return PerformGetRequest(FiwareUrl, "v2/entities" + parameters).Result.ToString();
        }

        public string GetTypes()
        {
            return PerformGetRequest(FiwareUrl, "v2/types").Result.ToString();
        }

        public string GetSubscriptions()
        {
            return PerformGetRequest(FiwareUrl, "v2/subscriptions").Result.ToString();
        }

        public string GetRegistrations()
        {
            return PerformGetRequest(FiwareUrl, "v2/registrations").Result.ToString();
        }
    }
}