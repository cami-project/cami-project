using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSS.Rules.Library
{
    public class EventValue
    {
        public EventValue() { }
    }

    public class EventUser{

        public int id;
    }
    public class EventJournal {

        public int id_enduser;
        public int[] id_caregivers;
    }
 

    public class SnoozedEventValue : EventValue
    {
        public EventUser user;
        public EventJournal journal;
    }



    public class AckEventValue : EventValue {

        public EventUser user;
        public EventUser journal;
        public string ack;

    }




    public class EventConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(Event).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {

            Console.WriteLine("read json");

            var e = JObject.Load(reader);

            var eType = e["content"]["name"].Value<String>();
            var convertedEvent = e.ToObject<Event>();

            object eventVal = null;

            switch (eType)
            {
                case "reminder_snoozed":
                    {
                        eventVal = new WeightValueInfo();
                        serializer.Populate(e["value_info"].CreateReader(), eventVal);
                        convertedEvent.content.val = (SnoozedEventValue)eventVal;
                        break;
                    }
                default:
                    {

                        throw new Exception("no conversion handler fo the event type");

                         break;
                    }

            }

            return convertedEvent;        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
