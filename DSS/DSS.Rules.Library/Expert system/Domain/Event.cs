using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DSS.Rules.Library
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


        public override string ToString(){
			 
            return string.Format("[Event: category={0}, content={1}, annotations={2}]", category, content, annotations);
		}


        public bool isReminderSent() {
            
            return content.name == "reminder_sent";
        }

        public bool isReminderACK(){

            return content.name == "reminder_acknowledged";
        }

        public bool isReminderSnoozed() {
            
            return content.name == "reminder_snoozed";
        }

        public bool isFall() {

            return content.name == "fall";
        }

        public bool isExerciseStarted(){

            return content.name == "exercise_started";
        }

        public bool isExerciseEnded() {

            return content.name == "exercise_ended";
        }
        public bool isMotionAlarm(){

            try
            {

               // Console.WriteLine( ((dynamic) (content.val)).alarm_motion);

                return (bool)content.val.alarm_motion;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        public string getUserURI(){


            if(isReminderSent()) {

                return string.Format("/api/v1/user/{0}/", content.val.user.id); 
            }

            else if(isReminderSnoozed()) {

                return string.Format("/api/v1/user/{0}/", content.val.user.id); 

            }
            else if(isReminderACK()) {
                
                return string.Format("/api/v1/user/{0}/", content.val.user.id); 
            }
            else if(isExerciseEnded() || isExerciseStarted()) {

                return string.Format("/api/v1/user/{0}/", content.val.user.id); 

            }



            throw new Exception("URI path for the user not speficied!");
        }
	}

}
