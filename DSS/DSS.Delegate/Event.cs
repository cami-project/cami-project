using System;
using Newtonsoft.Json;

namespace DSS.Delegate
{
    public class Event
    {

        public string Name { get; private set; }
        public string Type { get; private set; }
        public string Val { get; private set; }

        public Event(string name, string type, string val)
        {
            this.Name = name;
            this.Type = type;
            this.Val = val;
        }


        public bool Equals(Event obj)
        {
            return obj.Name == this.Name && obj.Type == this.Type;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Event);
        }

        public static bool operator == (Event a, Event b)
        {
            return a.Equals(b);
        }
		public static bool operator !=(Event a, Event b)
		{
            return !(a == b);
		}

        public override int GetHashCode()
        {
            return base.GetHashCode() + Name.GetHashCode() + Type.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[Event: Name={0}, Type={1}, Val={2}]", Name, Type, Val);
        }


        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
