using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WeatherApp.Models;

namespace WeatherApp.Services
{
    public static class GeoService
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<GeoData[]> GetCoordinates(string cityName, string apiKey)
        {
            string geoApiUrl = $"http://api.openweathermap.org/geo/1.0/direct?q={cityName}&limit=1&appid={apiKey}";
            Console.WriteLine($"GeoService URL: {geoApiUrl}");

            try
            {
                var response = await client.GetStringAsync(geoApiUrl);
                Console.WriteLine("GeoService Response received.");
                var geoData = JsonConvert.DeserializeObject<GeoData[]>(response);
                return geoData;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error retrieving coordinates: {e.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return null;
            }
        }
    }
}
