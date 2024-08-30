using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WeatherApp.Models;

namespace WeatherApp.Services
{
    public static class WeatherService
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<WeatherData> GetWeatherData(double latitude, double longitude, string apiKey)
        {
            string weatherApiUrl = $"https://api.openweathermap.org/data/3.0/onecall?lat={latitude}&lon={longitude}&exclude=minutely,hourly&appid={apiKey}";

            try
            {
                var response = await client.GetStringAsync(weatherApiUrl);
                Console.WriteLine("Raw JSON response: " + response); // Log raw JSON response
                var weatherData = JsonConvert.DeserializeObject<WeatherData>(response);
                return weatherData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving weather data: {ex.Message}");
                return null;
            }
        }
    }
}
