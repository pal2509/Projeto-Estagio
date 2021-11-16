using System;

#nullable disable

namespace API.Models
{
    public class WeatherData
    {
        public DateTime? Time { get; set; }
        public float? Temperature { get; set; }
        public float? Pressure { get; set; }
        public string WindDir { get; set; }
        public float? Windspeed { get; set; }
        public float? Humidity { get; set; }
        public float? Radiation { get; set; }
        public float? Precipitation { get; set; }

    }
}