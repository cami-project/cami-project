using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSS.FuzzyInference
{
class MeasurementConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return typeof(Measurement).IsAssignableFrom(objectType);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var measurement = JObject.Load(reader);

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
                        serializer.Populate(measurement["value_info"].CreateReader(), measurementVal);
						convertedMeasurement.value_info = (BloodPressureValueInfo)measurementVal;
						break;
					}
                default: {

                        measurementVal = new DefaultValueInfo();
						serializer.Populate(measurement["value_info"].CreateReader(), measurementVal);
						convertedMeasurement.value_info = (DefaultValueInfo)measurementVal;
                        break;
                    }
						


                    
					
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
