using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DSS.Delegate
{ 

	public class User
	{
		public string name { get; set; }
		public string uri { get; set; }
	}

	public class Room
	{
		public string name { get; set; }
	}

	public class Value
	{
		public User user { get; set; }
		public Room room { get; set; }
        //This is added by me
        public float numVal { get; set; }
	}

	public class Content
	{
        [JsonProperty("uuid")]
        public string uuid { get; set; }

        public string name { get; set; }

        [JsonProperty("in_reply_to")]
        public string inReplyTo { get; set; }

        public string value_type { get; set; }

        [JsonProperty("value")]
        //public Value val { get; set; }
        public dynamic val { get; set; }

	}

	public class TemporalValidity
	{
		public int start_ts { get; set; }
		public int end_ts { get; set; }
	}

	public class Annotations
	{
		public long timestamp { get; set; }
		//public List<string> source { get; set; }
		public dynamic source { get; set; }

		public int certainty { get; set; }
		public TemporalValidity temporal_validity { get; set; }
	}

	public class Event
	{
		public string category { get; set; }
		public Content content { get; set; }
		public Annotations annotations { get; set; }

        public Event()
        {

        }

        public Event(string category, Content content, Annotations annotations) 
        {
            this.category = category;
            this.content = content;
            this.annotations = annotations;
        }

		public Event(string category)
		{
			this.category = category;

		}


		public bool Equals(Event obj)
	    {
            return obj.category == this.category ;
	    }
        public override bool Equals(object obj)
        {
            return Equals(obj as Event);
        }

		public static bool operator== (Event a, Event b)
        {
            return a.Equals(b);
        }
		public static bool operator !=(Event a, Event b)
        { 
            return !(a == b);
		}

        public override string ToString()
        {
			 return string.Format("[Event: category={0}, content={1}, annotations={2}]", category, content, annotations);

			//return string.Format("[Event: category={0}]", category);

		}

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}

	}




	// The old event 
	//  public class Event
	//  {

	//      public string Name { get; private set; }
	//      public string Type { get; private set; }
	//      public string Val { get; private set; }
	//      public string Device { get; private set;}

	//      public Event(string name, string type, string val, string device)
	//      {
	//          this.Name = name;
	//          this.Type = type;
	//          this.Val = val;
	//          this.Device = device;

	//      }


	//      public bool Equals(Event obj)
	//      {
	//          return obj.Name == this.Name && obj.Type == this.Type && obj.Device == this.Device;
	//      }

	//      public override bool Equals(object obj)
	//      {
	//          return Equals(obj as Event);
	//      }

	//      public static bool operator == (Event a, Event b)
	//      {
	//          return a.Equals(b);
	//      }
	//public static bool operator !=(Event a, Event b)
	//{
	//          return !(a == b);
	//}

	//    public override int GetHashCode()
	//    {
	//        return base.GetHashCode() + Name.GetHashCode() + Type.GetHashCode() + Device.GetHashCode();
	//    }

	//    public override string ToString()
	//    {
	//        return string.Format("[Event: Name={0}, Type={1}, Val={2}, Device={3}]", Name, Type, Val, Device);
	//    }


	//    public string ToJson()
	//    {
	//        return JsonConvert.SerializeObject(this);
	//    }
	//}











}
