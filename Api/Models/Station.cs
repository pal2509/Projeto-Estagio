using System;
using System.Collections.Generic;

#nullable disable

namespace API.Models
{
    public partial class Station
    {
        public Guid Stationid { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Name { get; set; }
        public string Createdby { get; set; }
        public DateTime Createdat { get; set; }
        public string Updatedby { get; set; }
        public DateTime Updatedat { get; set; }
        public string Tempunit { get; set; }
        public string Windspeedunit { get; set; }
        public string Pressureunit { get; set; }
        public string Precipitationunit { get; set; }
        public string Radiationunit { get; set; }
        public string Leafwetnessunit { get; set; }
        public string Soiltempunit { get; set; }
        public string Soilmoistunit { get; set; }
    }
}
