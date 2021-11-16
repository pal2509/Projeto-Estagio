using System;
using System.Collections.Generic;

#nullable disable

namespace API.Models
{
    public partial class Stationrecord
    {
        public Guid? Stationid { get; set; }
        public DateTime Time { get; set; }
        public float? Temperature { get; set; }
        public float? Humidity { get; set; }
        public float? Windspeed { get; set; }
        public string Winddir { get; set; }
        public float? Pressure { get; set; }
        public float? Precipitation { get; set; }
        public float? Radiation { get; set; }
        public float? Leafwetness { get; set; }
        public float? Soilmoisture1 { get; set; }
        public float? Soilmoisture2 { get; set; }
        public float? Soilmoisture3 { get; set; }
        public float? Soiltemperature1 { get; set; }
        public float? Soiltemperature2 { get; set; }
        public float? Soiltemperature3 { get; set; }
        public float? Customd1 { get; set; }
        public float? Customd2 { get; set; }
        public float? Customd3 { get; set; }
        public float? Customd4 { get; set; }
        public float? Customd5 { get; set; }
        public string Customt1 { get; set; }
        public string Customt2 { get; set; }
        public string Customt3 { get; set; }
        public string Customt4 { get; set; }
        public string Customt5 { get; set; }

        //public virtual Station Station { get; set; }
    }
}
