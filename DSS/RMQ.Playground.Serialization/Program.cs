using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using DSS.FuzzyInference;
using DSS.Delegate;

namespace RMQ.Playground.Serialization
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Testing serialization");

            string jsonBPStr = @"{""measurement_type"": ""blood_pressure"", 
                                ""unit_type"" : ""mmHg"", 
                                ""timestamp"": 1505827500, 
                                ""user"": ""/api/v1/user/2/"", 
                                ""device"": ""/api/v1/device/1/"", 
                                ""value_info"": {""systolic"" : 117, ""diastolic"": 67, ""heartrate"": 73 } }";

            string jsonWeightStr = @"{""measurement_type"": ""weight"", 
                                ""unit_type"" : ""mmHg"", 
                                ""timestamp"": 1505827500, 
                                ""user"": ""/api/v1/user/2/"", 
                                ""device"": ""/api/v1/device/1/"", 
                                ""value_info"": {""weight"" : 74.1} }";

            string jsonPulseStr = @"{""measurement_type"": ""pulse"", 
                                ""unit_type"" : ""mmHg"", 
                                ""timestamp"": 1505827500, 
                                ""user"": ""/api/v1/user/2/"", 
                                ""device"": ""/api/v1/device/1/"", 
                                ""value_info"": {""value"": 65 } }";

            string eventJsonStr = @"{""category"": ""USER_ENVIRONMENT"",
                                     ""content"": 
                                        {""name"": ""presence"",
                                         ""value_type"": ""complex"",
                                         ""value"": {""alarm_motion"":false, ""sensor_luminance"":64, ""sensor_temperature"":27.5, ""alarm_tamper"":false,""battery_level"":85}},
                                     ""annotations"": {""timestamp"":1507902069, ""source"":{""gateway"":""/api/v1/gateway/1/"", ""sensor"": ""/api/v1/device/9/""}
                                    }}";

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new MeasurementConverter());

            var bpObj = JsonConvert.DeserializeObject<Measurement>(jsonBPStr, settings);
            var weightObj = JsonConvert.DeserializeObject<Measurement>(jsonWeightStr, settings);
            var pulseObj = JsonConvert.DeserializeObject<Measurement>(jsonPulseStr, settings);

            try
            {
                var eventObj = JsonConvert.DeserializeObject<Event>(eventJsonStr);
                Console.WriteLine("Sensor source is: " + eventObj.annotations.source["sensor"]);
                Console.WriteLine("Motion detected status: " + eventObj.content.val["alarm_motion"]);
            }
            catch (JsonException ex)
            {
                Console.WriteLine(ex);
            }


            BloodPressureValueInfo bpVal = (BloodPressureValueInfo)bpObj.value_info;
            Console.WriteLine("systolic: " + bpVal.systolic + " diastolic: " + bpVal.diastolic + " pulse: " + bpVal.pulse);

            WeightValueInfo weightVal = (WeightValueInfo)weightObj.value_info;
            Console.WriteLine("weight: " + weightVal.Value);

            PulseValueInfo pulseVal = (PulseValueInfo)pulseObj.value_info;
            Console.WriteLine("pulse: " + pulseVal.Value);

            Console.WriteLine("Reseerialized Measurement of type: " + bpObj.measurement_type);
            Console.WriteLine(JsonConvert.SerializeObject(bpObj));

            Console.WriteLine("Reseerialized Measurement of type: " + weightObj.measurement_type);
            Console.WriteLine(JsonConvert.SerializeObject(weightObj));

            Console.WriteLine("Reseerialized Measurement of type: " + pulseObj.measurement_type);
            Console.WriteLine(JsonConvert.SerializeObject(pulseObj));

            Console.ReadLine();
        }
    }
}
