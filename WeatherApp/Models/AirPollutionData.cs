using System;
using System.Collections.Generic;

namespace WeatherApp.Models
{
    public class AirPollutionData
    {
        public Coord Coord { get; set; }
        public List<AirPollutionList> List { get; set; }
    }

    public class Coord
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
    }

    public class AirPollutionList
    {
        public long Dt { get; set; }
        public AirQualityMain Main { get; set; }
        public Components Components { get; set; }
    }

    public class AirQualityMain
    {
        public int Aqi { get; set; }
    }

    public class Components
    {
        public double Co { get; set; }
        public double No { get; set; }
        public double No2 { get; set; }
        public double O3 { get; set; }
        public double So2 { get; set; }
        public double Pm2_5 { get; set; }  // Changed to Pm25 for consistency
        public double Pm10 { get; set; }
        public double Nh3 { get; set; }
    }
}
