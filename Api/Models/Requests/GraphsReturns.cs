using System;

#nullable disable

namespace API.Models
{
    public class Graph
    {
        public DateTime? time { get; set; }
        public float? value { get; set; }
    }

    public class GraphWind
    {
        public DateTime? time { get; set; }
        public float? value { get; set; }
        public string direction { get; set; }
    }

    public class GraphSoil
    {
        public DateTime? time { get; set; }
        public float? moist1 { get; set; }
        public float? moist2 { get; set; }
        public float? moist3 { get; set; }
        public float? temp1 { get; set; }
        public float? temp2 { get; set; }
        public float? temp3 { get; set; }
    }

}
