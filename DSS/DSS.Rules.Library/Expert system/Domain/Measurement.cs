using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSS.Rules.Library
{

    public class Measurement
    {
        public string measurement_type { get; set; }
        public string unit_type { get; set; }
        public int timestamp { get; set; }
        public string user { get; set; }
        public string device { get; set; }

        public ValueInfoBase value_info { get; set; }
        public string gateway_id { get; set; }
        public bool ok { get; set; }
        public string id { get; set; }
        public string resource_uri { get; set; }


        public bool isSteps(){

            return measurement_type == "steps";
        }

        public bool isPulse(){

            return measurement_type == "pulse";
        }

        public bool isPulseOK() {

            var val = getValue();

            int min = 50, max = 120;

            if (val < min || val > max)
               return false;
           
            return true;
        }

        public bool isLessThen(int val) {
            
            return getValue() < val;
        }
        public bool isBetween(int x, int y) {

            return getValue() >= x && getValue() <= y;
        }

        public bool isBiggerThen(int val){

            return getValue() > val;
        }

        public bool isNight() {
                
            TimeZoneInfo localTz = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
            DateTime pulseDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(this.timestamp);
            DateTime localizedPulseDateTime = UnixTimeStampToDateTime(this.timestamp, localTz);

            return pulseDateTime.AddHours(1) > DateTime.UtcNow && localizedPulseDateTime.Hour >= 6;

        }
        private DateTime UnixTimeStampToDateTime(long unixTimeStamp, TimeZoneInfo tzInfo){
            
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);

            dtDateTime = TimeZoneInfo.ConvertTime(dtDateTime, TimeZoneInfo.Utc, tzInfo);
            return dtDateTime;
        }



        public float getValue(){
            
            if(isWeight()){

                return ((WeightValueInfo)value_info).Value; 
            }
            else if(isPulse()){

                return ((PulseValueInfo)value_info).Value;
            }

            return -1;
        }
        public bool isWeight(){
            
            return measurement_type == "weight";
        }

        public bool differentIsBigger(float kg, float threshold)
        {
            
            Console.WriteLine(getValue() + " / " + kg);

            if (getValue() > kg)
            {
                return Math.Abs(getValue() - kg) > threshold;
            }

            return false;
        }

        public bool differenceIsLess(float kg, float threshold)
        {

            Console.WriteLine("diff is les" + getValue());

            if (getValue() < kg)
            {
                return Math.Abs(getValue() - kg) > threshold;
            }

            return false;
        }

        public float differenceInKg(float kg)
        {

            return (float)Math.Floor(Math.Abs(getValue() - kg));
        }


    }


    public class ValueInfoBase
    {
        public ValueInfoBase() { }
    }

    public class DefaultValueInfo : ValueInfoBase
    {
        [JsonProperty("value")]
        public int Value { get; set; }
    }

    public class PulseValueInfo : ValueInfoBase
    {
        [JsonProperty("value")]
        public int Value { get; set; }
    }

    public class StepsValueInfo : ValueInfoBase
    {
        [JsonProperty("value")]
        public int Value { get; set; }

        public int start_timestamp { get; set; }

        public int end_timestamp { get; set; }
    }

    public class WeightValueInfo : ValueInfoBase
    {
        [JsonProperty("value")]
        public float Value { get; set; }

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        private List<string> AlternateWeightPropertyNames = new List<string> { "weight" };

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            // alternate property names for pulse are added to additionalData
            // we check if there are any, set the original pulse variable and then clear out the dictionary;
            foreach (string altPropName in AlternateWeightPropertyNames)
            {
                if (_additionalData.ContainsKey(altPropName))
                {
                    Value = (float)_additionalData[altPropName];
                    break;
                }
            }

            // clear out _additionalData
            _additionalData.Clear();
        }

        public WeightValueInfo()
        {
            _additionalData = new Dictionary<string, JToken>();
        }
    }

    public class BloodPressureValueInfo : ValueInfoBase
    {
        public int pulse { get; set; }

        public int diastolic { get; set; }

        public int systolic { get; set; }

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        private List<string> AlternatePulsePropertyNames = new List<string> { "pulserate", "heartrate" };

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            // alternate property names for pulse are added to additionalData
            // we check if there are any, set the original pulse variable and then clear out the dictionary;
            foreach (string altPropName in AlternatePulsePropertyNames)
            {
                if (_additionalData.ContainsKey(altPropName))
                {
                    pulse = (int)_additionalData[altPropName];
                    break;
                }
            }

            // clear out _additionalData
            _additionalData.Clear();
        }


        public BloodPressureValueInfo()
        {
            _additionalData = new Dictionary<string, JToken>();
        }

    }



}
