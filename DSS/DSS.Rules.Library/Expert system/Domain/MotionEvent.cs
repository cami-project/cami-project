using System;
using Newtonsoft.Json;

namespace DSS.Rules.Library
{
    public class MotionEvent
    {
        public class Value
        {
            public bool alarm_motion { get; set; }
            public bool alarm_tamper { get; set; }
            public int sensor_luminance { get; set; }
            public int sensor_temperature { get; set; }

        }

        public class Content
        {
            public string name { get; set; }
            public string value_type { get; set; }

            [JsonProperty("value")]
            public Value val { get; set; }
        }

        public class Source
        {
            public string gateway { get; set; }
            public string sensor { get; set; }
        }

        public class Annotations
        {
            public int timestamp { get; set; }
            public Source source { get; set; }
        }

        public string category { get; set; }
        public Content content { get; set; }
        public Annotations annotations { get; set; }
     

        public bool isKitchen() {
            
            return annotations.source.sensor == "/api/v1/device/1/";
        }

        public bool isBathroom() {

            return annotations.source.sensor == "/api/v1/device/2/";
        }

        public string getGateway() {

            return annotations.source.gateway;
        }

        public string getLocationName() 
        {
            if (isKitchen()) return "KITCHEN";
            if (isBathroom()) return "BATHROOM";

            return "UNKNOWN";
        }
    }
}
