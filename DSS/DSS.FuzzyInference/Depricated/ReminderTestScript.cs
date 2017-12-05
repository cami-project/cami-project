using System;
namespace DSS.FuzzyInference
{
    public class ReminderTestScript
    {
        public ReminderTestScript()
        {

            //var store = new StoreAPI("http://cami-store:8008");
            //var store = new StoreAPI("http://141.85.241.224:8008");


            //var je = store.PushJournalEntry("/api/v1/user/2/", "blood_pressure", "low", "blood pressure", "blood_pressure");


            //var reminderEvent = new Event()
            //{
            //    category = "USER_NOTIFICATIONS",
            //    content = new Content()
            //    {
            //        uuid = Guid.NewGuid().ToString(),
            //        name = "reminder_sent",
            //        value_type = "complex",
            //        val = new Dictionary<string, dynamic>()
            //            {
            //                 { "user", new Dictionary<string, int>() { {"id", 2 } } },
            //                 { "journal", new Dictionary<string, dynamic>() {
            //                     { "id_enduser", 2 },
            //                     { "id_caregivers", new [] {2}}
            //                 } },
            //            }
            //    },
            //    annotations = new Annotations()
            //    {
            //        timestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
            //        source = "DSS"
            //    }
            //};


            //var reminderACK = new Event()
            //{
            //    category = "USER_NOTIFICATIONS",
            //    content = new Content()
            //    {
            //        uuid = Guid.NewGuid().ToString(),
            //        name = "reminder_acknowledged",
            //        value_type = "complex",
            //        val = new Dictionary<string, dynamic>()
            //        {   { "ack" , "snooze"},
            //            { "user", new Dictionary<string, int>() { {"id", 2 } } },
            //            { "journal", new Dictionary<string, dynamic>() {{ "id", je.id }} },
            //        }
            //    },
            //    annotations = new Annotations()
            //    {
            //        timestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
            //        source = "ios_app"
            //    }
            //};




            //var mesuremtent = new Measurement()
            //{
            //    unit_type = "bpm",
            //    device = "/api/v1/device/2/",
            //    ok = true,
            //    user = "/api/v1/user/2/",
            //    value_info = new BloodPressureValueInfo()
            //    {
            //        diastolic = 10,
            //        pulse = 100,
            //        systolic = 5
            //    },
            //    measurement_type = "blood_pressure",
            //    gateway_id = null,
            //    timestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds

            //};



            //var handler = new ReminderHandler();
            //handler.Handle(JsonConvert.SerializeObject(reminderEvent));
            //handler.Handle(JsonConvert.SerializeObject(reminderACK));


            //store.PushMeasurement(JsonConvert.SerializeObject(mesuremtent));


        }
    }
}
