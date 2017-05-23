using System;
namespace DSS.Delegate
{
    public class Event
    {

        public string Name { get; private set; }
        public string Type { get; private set; }
        public string Value { get; private set; }

        public Event(string name, string type, string val)
        {
            this.Name = name;
            this.Type = type;
            this.Value = val;
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
            return string.Format("[Event: Name={0}, Type={1}, Value={2}]", Name, Type, Value);
        }

    }
}
