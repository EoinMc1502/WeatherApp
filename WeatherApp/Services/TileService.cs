using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using WeatherApp.Models;

namespace WeatherApp.Services
{
    public class TileService
    {
        private readonly HttpClient _client;
        private readonly string _apiKey;

        public TileService(HttpClient client, string apiKey)
        {
            _client = client;
            _apiKey = apiKey;
        }

        public async Task FetchAndSaveTileImage(TileRequest request, string filePath)
        {
            string url = $"https://tile.openweathermap.org/map/{request.Layer}/{request.ZoomLevel}/{request.XCoord}/{request.YCoord}.png?appid={_apiKey}";

            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            using (var fs = new FileStream(filePath, FileMode.CreateNew))
            {
                await response.Content.CopyToAsync(fs);
            }
        }
    }
}

