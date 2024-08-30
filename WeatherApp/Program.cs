using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WeatherApp.Services;
using WeatherApp.Models;

namespace WeatherApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Set up configuration to read from appsettings.json
            Console.WriteLine("Setting up configuration...");
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string apiKey = configuration["OpenWeatherMap:ApiKey"];
            Console.WriteLine($"API Key loaded: {apiKey}");

            // Ensure the API key is not empty
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Console.WriteLine("API Key is missing or empty. Please check appsettings.json.");
                return;
            }

            // Get user input for the city name
            Console.Write("Enter the city name: ");
            string cityName = Console.ReadLine();
            Console.WriteLine($"City name entered: {cityName}");

            // Get latitude and longitude from the Geocoding API
            Console.WriteLine("Calling GeoService to get coordinates...");
            var geoResponse = await GeoService.GetCoordinates(cityName, apiKey);
            if (geoResponse == null || geoResponse.Length == 0)
            {
                Console.WriteLine("Location not found.");
                return;
            }

            double latitude = geoResponse[0].Lat;
            double longitude = geoResponse[0].Lon;
            Console.WriteLine($"Coordinates found: Latitude = {latitude}, Longitude = {longitude}");

            // Call the Weather API using the obtained coordinates
            Console.WriteLine("Calling WeatherService to get weather data...");
            var weatherData = await WeatherService.GetWeatherData(latitude, longitude, apiKey);
            if (weatherData == null)
            {
                Console.WriteLine("Failed to retrieve weather data.");
                return;
            }

            // Call the Weather API using the obtained coordinates
            Console.WriteLine("Calling AirPollutionService to get weather data...");
            var AirPollutionData = await AirPollutionService.GetAirQuality(latitude, longitude, apiKey);
            if (AirPollutionData == null)
            {
                Console.WriteLine("Failed to retrieve air pollution data.");
                return;
            }


            // Display relevant information
            Console.WriteLine("\nCurrent Weather Information:");
            Console.WriteLine($"Location: {cityName} (Latitude: {latitude}, Longitude: {longitude})");
            Console.WriteLine($"Current Climate: {weatherData.Current.Weather[0].Main} ({weatherData.Current.Weather[0].Description})");
            Console.WriteLine($"Current Temperature: {weatherData.Current.Temp - 273.15:F1}°C"); // Convert from Kelvin to Celsius
            Console.WriteLine($"Feels Like Temperature: {weatherData.Current.FeelsLike - 273.15:F1}°C"); // Convert from Kelvin to Celsius
            Console.WriteLine($"Sunrise: {DateTimeOffset.FromUnixTimeSeconds(weatherData.Current.Sunrise).ToString("HH:mm:ss")}");
            Console.WriteLine($"Sunset: {DateTimeOffset.FromUnixTimeSeconds(weatherData.Current.Sunset).ToString("HH:mm:ss")}");
            Console.WriteLine($"Pressure: {weatherData.Current.Pressure} hPa");
            Console.WriteLine($"Humidity: {weatherData.Current.Humidity}%");
            /*foreach(var item in AirPollutionData.List)
            {
                Console.WriteLine($"Air Quality Index (AQI): {item.Main.Aqi}");
            }*/
            int aqi = AirPollutionData.List[0].Main.Aqi;
            string aqiDescription;

            switch (aqi)
            {
                case 1:
                    aqiDescription = "Good";
                    break;
                case 2:
                    aqiDescription = "Fair";
                    break;
                case 3:
                    aqiDescription = "Moderate";
                    break;
                case 4:
                    aqiDescription = "Poor";
                    break;
                case 5:
                    aqiDescription = "Very Poor";
                    break;
                default:
                    aqiDescription = "Unknown";
                    break;
            }
            Console.WriteLine($"AQI: {aqi} - {aqiDescription}");

            Console.WriteLine($"Dew Point: {weatherData.Current.DewPoint - 273.15:F1}°C"); // Convert from Kelvin to Celsius
            if (weatherData.Current.Temp - 273.15 >= 27)
            {
                double temperatureC = weatherData.Current.Temp - 273.15;
                double temperatureF = temperatureC * 9 / 5 + 32;
                double humidity = weatherData.Current.Humidity;

                double heatIndexF = -42.379 + 2.04901523 * temperatureF + 10.14333127 * humidity
                                  - 0.22475541 * temperatureF * humidity - 0.00683783 * Math.Pow(temperatureF, 2)
                                  - 0.05481717 * Math.Pow(humidity, 2) + 0.00122874 * Math.Pow(temperatureF, 2) * humidity
                                  + 0.00085282 * temperatureF * Math.Pow(humidity, 2) - 0.00000199 * Math.Pow(temperatureF, 2) * Math.Pow(humidity, 2);

                // Convert back to Celsius
                double heatIndexC = (heatIndexF - 32) * 5 / 9;
                Console.WriteLine($"Heat Index: {heatIndexC:F1}°C");
            }

            Console.WriteLine($"UV Index: {weatherData.Current.Uvi}");
            Console.WriteLine($"Cloud Coverage: {weatherData.Current.Clouds}%");
            Console.WriteLine($"Visibility: {weatherData.Current.Visibility} meters");
            Console.WriteLine($"Wind Speed: {weatherData.Current.WindSpeed} m/s");
            if (weatherData.Current.Temp - 273.15 <= 10)
            {
                double temperatureC = weatherData.Current.Temp - 273.15;
                double windSpeedKmh = weatherData.Current.WindSpeed * 3.6;
                double windChill = 13.12 + 0.6215 * temperatureC - 11.37 * Math.Pow(windSpeedKmh, 0.16) + 0.3965 * temperatureC * Math.Pow(windSpeedKmh, 0.16);
                Console.WriteLine($"Wind Chill: {windChill:F1}°C");
            }

            if (weatherData.Alerts != null && weatherData.Alerts.Count > 0)
            {
                Console.WriteLine("ALERTS:");
                foreach (var incident in weatherData.Alerts)
                {
                    if (!string.IsNullOrEmpty(incident.Event))
                    {
                        Console.WriteLine($"Sender Name: {incident.SenderName} - Event: {incident.Event} - Start: {DateTimeOffset.FromUnixTimeSeconds(incident.Start).DateTime} - End: {DateTimeOffset.FromUnixTimeSeconds(incident.End).DateTime} - Description: {incident.Description}");
                    }
                }
            }
            else
            {
                Console.WriteLine("No alerts at this time.");
            }

            // Optionally, you can display the daily forecast as well
            Console.WriteLine("\nDaily Forecast:");
            foreach (var day in weatherData.Daily)
            {
                var dateTime = DateTimeOffset.FromUnixTimeSeconds(day.Dt).DateTime;
                Console.WriteLine($"\nDate: {dateTime.ToShortDateString()}");
                Console.WriteLine($"Summary: {day.Summary}");
                Console.WriteLine($"Day Temperature: {day.Temp.Day - 273.15:F1}°C"); // Convert from Kelvin to Celsius
                Console.WriteLine($"Min Temperature: {day.Temp.Min - 273.15:F1}°C"); // Convert from Kelvin to Celsius
                Console.WriteLine($"Max Temperature: {day.Temp.Max - 273.15:F1}°C"); // Convert from Kelvin to Celsius
                Console.WriteLine($"Night Temperature: {day.Temp.Night - 273.15:F1}°C"); // Convert from Kelvin to Celsius
                Console.WriteLine($"Evening Temperature: {day.Temp.Eve - 273.15:F1}°C"); // Convert from Kelvin to Celsius
                Console.WriteLine($"Morning Temperature: {day.Temp.Morn - 273.15:F1}°C"); // Convert from Kelvin to Celsius
                Console.WriteLine($"Feels Like Temperature: {day.FeelsLike.Day - 273.15:F1}°C"); // Convert from Kelvin to Celsius
                Console.WriteLine($"Sunrise: {DateTimeOffset.FromUnixTimeSeconds(day.Sunrise).ToString("HH:mm:ss")}");
                Console.WriteLine($"Sunset: {DateTimeOffset.FromUnixTimeSeconds(day.Sunset).ToString("HH:mm:ss")}");
                Console.WriteLine($"Pressure: {day.Pressure} hPa");
                Console.WriteLine($"Humidity: {day.Humidity}%");
                Console.WriteLine($"Dew Point: {day.DewPoint - 273.15:F1}°C"); // Convert from Kelvin to Celsius
                Console.WriteLine($"Wind Speed: {day.WindSpeed} m/s");
                Console.WriteLine($"Wind Direction: {day.WindDeg}°");
                Console.WriteLine($"Wind Gust: {day.WindGust} m/s");
                Console.WriteLine($"Cloud Coverage: {day.Clouds}%");
                Console.WriteLine($"Chance of Rain: {day.Pop * 100}%");
                if (day.Rain.HasValue)
                {
                    Console.WriteLine($"Rain Volume: {day.Rain} mm");
                }
                Console.WriteLine($"UV Index: {day.Uvi}");

                if (weatherData.Hourly != null)
                {
                    Console.WriteLine("\nHourly Forecast:");
                    foreach (var hour in weatherData.Hourly)
                    {
                        var HourlyDateTime = DateTimeOffset.FromUnixTimeSeconds(hour.Dt).DateTime;
                        Console.WriteLine($"\nDate: {HourlyDateTime.ToShortDateString()}");
                        Console.WriteLine($"Temperature: {hour.Temp - 273.15:F1}°C");
                        Console.WriteLine($"Feels Like Temperature: {hour.FeelsLike - 273.15:F1}°C");
                        Console.WriteLine($"Pressure: {hour.Pressure} hPa");
                        Console.WriteLine($"Humidity: {hour.Humidity}%");
                        Console.WriteLine($"Dew Point: {hour.DewPoint - 273.15:F1}°C"); // Convert from Kelvin to Celsius
                        Console.WriteLine($"UV Index: {hour.Uvi}");
                        Console.WriteLine($"Cloud Coverage: {hour.Clouds}%");
                        Console.WriteLine($"Visibility: {hour.Visibility} meters");
                        Console.WriteLine($"Wind Speed: {hour.WindSpeed} m/s");
                        Console.WriteLine($"Wind Direction: {hour.WindDeg}°");
                        Console.WriteLine($"Wind Gust: {hour.WindGust} m/s");
                        Console.WriteLine($"Weather: \n ID - {hour.Weather[0].Id} \n Main - {hour.Weather[0].Main} \n Description - {hour.Weather[0].Description} \n Icon - {hour.Weather[0].Icon}");
                        Console.WriteLine($"Chance of Rain: {hour.Pop * 100}%");
                        if (hour.Rain != null)
                        {
                            Console.WriteLine($"Rain Volume: {hour.Rain.OneHour} mm");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("\nNo Hourly Forecast Available");
                }
            }
        }
    }
}
