using System;
namespace WeatherApp.Models
{
    public class TileRequest
    {
        public string Layer { get; set; }
        public int ZoomLevel { get; set; }
        public int XCoord { get; set; }
        public int YCoord { get; set; }
    }

}

