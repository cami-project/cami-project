using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DSS.FuzzyInference;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace RMQ.Playground.Serialization
{
    class MeasurementConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(Measurement).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject measurement = JObject.Load(reader);

            var measurementType = measurement["measurement_type"].Value<String>();
            var convertedMeasurement = measurement.ToObject<Measurement>();

            object measurementVal = null;

            switch (measurementType)
            {
                case "weight":
                    {
                        measurementVal = new WeightValueInfo();
                        serializer.Populate(measurement["value_info"].CreateReader(), measurementVal);
                        convertedMeasurement.value_info = (WeightValueInfo)measurementVal;
                        break;
                    }
                case "pulse":
                    {
                        measurementVal = new PulseValueInfo();
                        serializer.Populate(measurement["value_info"].CreateReader(), measurementVal);
                        convertedMeasurement.value_info = (PulseValueInfo)measurementVal;
                        break;
                    }
                case "blood_pressure":
                    {
                        measurementVal = new BloodPressureValueInfo();

                        // set custom BPValueContractResolver contract resolver
                        //serializer.ContractResolver = new BPValueContractResolver();
                        //serializer.Converters.Insert(0, new BloodPressureValueConverter());

                        //JsonSerializer bpValSerializer = JsonSerializer.CreateDefault();
                        //bpValSerializer.Converters.Add(new BloodPressureValueConverter());

                        serializer.Populate(measurement["value_info"].CreateReader(), measurementVal);
                        convertedMeasurement.value_info = (BloodPressureValueInfo)measurementVal;
                        break;
                    }
                default:
                    throw new ArgumentException("Invalid measurement type: " + measurementType);
            }

            return convertedMeasurement;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    //class BPValueContractResolver : DefaultContractResolver
    //{
    //    private Dictionary<string, string> PropertyMappings { get; set; }

    //    public BPValueContractResolver()
    //    {
    //        this.PropertyMappings = new Dictionary<string, string>
    //        {
    //            {"pulse", "pulse"},
    //            {"pulserate", "pulse"},
    //            {"heartrate", "pulse"}
    //        };
    //    }

    //    protected override string ResolvePropertyName(string propertyName)
    //    {
    //        string resolvedName = null;
    //        var resolved = this.PropertyMappings.TryGetValue(propertyName, out resolvedName);
    //        return (resolved) ? resolvedName : base.ResolvePropertyName(propertyName);
    //    }
    //}
}
