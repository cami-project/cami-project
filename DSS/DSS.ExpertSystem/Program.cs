using System;
using NRules;
using NRules.Fluent;
using NRules.Fluent.Dsl;

namespace DSS.ExpertSystem
{
    class Program
    {
        public static void Main(string[] args)
        {
			//Load rules
			var repository = new RuleRepository();
            repository.Load(x => x.From(typeof(FallRule).Assembly));

			//Compile rules
			var factory = repository.Compile();

			//Create a working session
			var session = factory.CreateSession();

            //Load domain model
            var fallEvent = new Event("Fall", "High");

			//Insert facts into rules engine's memory
            session.Insert(fallEvent);

			//Start match/resolve/act cycle
			session.Fire();
        }
    }


    public class Event 
    {
        public string Name { get; set; }
		public string Value { get; set; }


        public Event (string name, string val)
        {
            this.Name = name;
            this.Value = val;
        }


        public override string ToString()
        {
            return string.Format("[Event: Name={0}, Value={1}]", Name, Value);
        }
    }

    public class AlarmService
    {
        public static void Invoke(string message)
        {
            Console.WriteLine(string.Format( "Alarm: {0}", message));
        }
    }

    public class FallRule : Rule
    {
        public override void Define()
        {

            Event e = null;

            When().Match<Event>(() => e, newEvent => newEvent.Name == "Fall" && (newEvent.Value == "High" || newEvent.Value == "Medium" ));

            Then().Do(_ => AlarmService.Invoke(e.ToString()));
        }
    }

}
