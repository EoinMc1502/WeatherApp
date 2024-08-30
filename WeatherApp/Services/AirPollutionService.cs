using System;
using Newtonsoft.Json;
using WeatherApp.Models;
using static System.Net.WebRequestMethods;

namespace WeatherApp.Services
{
	public static class AirPollutionService
	{
        private static readonly HttpClient client = new HttpClient();

        public static async Task<AirPollutionData> GetAirQuality(double latitude, double longitude, string apiKey)
        {
            string AirPollutionApiUrl = $"http://api.openweathermap.org/data/2.5/air_pollution?lat={latitude}&lon={longitude}&appid={apiKey}";
            Console.WriteLine($"AirPollutionService URL: {AirPollutionApiUrl}");

            try
            {
                var response = await client.GetStringAsync(AirPollutionApiUrl);
                Console.WriteLine("AirPollutionService Response received.");
                Console.WriteLine("Raw JSON response: " + response); // Log raw JSON response

                var AirPollutionData = JsonConvert.DeserializeObject<AirPollutionData>(response);
                return AirPollutionData;
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

