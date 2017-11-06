using System;
using System.Collections.Generic;

namespace DSS.FuzzyInference
{
    public class Loc
    {

        public static string MSG = "MSG", DES = "DES", EN = "EN", RO = "RO", PO = "PO", CAREGVR= "CAREGVR", USR = "USR";


        public static string STEPS_LESS_1000 = "STEPS_LESS_1000";
        public static string STEPS_BETWEEN_1000_2000 = "STEPS_BETWEEN_1000_2000";
        public static string STEPS_BIGGER_6000 = "STEPS_BIGGER_6000";

        public static string REMINDER_SENT = "REMINDER_SENT";
        public static string REMINDER_POSTPONED = "REMINDER_POSTPONED";
        public static string REMINDER_IGNORED = "REMINDER_IGNORED";
        public static string MEASUREMENT_IGNORED = "MEASUREMENT_IGNORED";

        public static string WEIGHT_DEC = "WEIGHT_DEC";
        public static string WEIGHT_INC = "WEIGHT_INC";

        public static string PULSE_LOW = "PULSE_LOW";
        public static string PULSE_MID_LOW = "PULSE_MID_LOW";
        public static string PULSE_MEDIUM = "PULSE_MEDIUM";
        public static string PULSE_HIGH = "PULSE_HIGH";


        private static Dictionary<string, string> text = new Dictionary<string, string>(){

            //STEPS
            { EN + MSG + STEPS_LESS_1000, "Low step count alert!" },
            { EN + DES + STEPS_LESS_1000, "Your loved one has walked very few steps today. Call them to ask they move more." },
           
            { EN + MSG + STEPS_BETWEEN_1000_2000, "Hey! Your number of steps for today is quite low: {0}" },
            { EN + DES + STEPS_BETWEEN_1000_2000, "Why not take a short walk?" },

            { EN + MSG + STEPS_BIGGER_6000, "Hey! Good job, today you made {0} steps." },
            { EN + DES + STEPS_BIGGER_6000, "Today you made {0} steps! Keep it up!”" },

            //BLOOD PRESSURE
            { EN + USR + MSG + REMINDER_SENT, "Time for your morning blood pressure measurement!" },
            { EN + USR + DES + REMINDER_SENT, "Please take your blood pressure before breakfast." },
            { EN + CAREGVR + MSG + REMINDER_SENT, "Reminder for morning blood pressure measurement sent!" },
            { EN + CAREGVR + DES + REMINDER_SENT, "Check on your loved one to see that he took the recommended blood pressure measurement." },

            { EN + CAREGVR + MSG + REMINDER_POSTPONED, "Your loved one postponed the blood pressure measurements." },
            { EN + CAREGVR + DES + REMINDER_POSTPONED, "Please take action and call to remind them of the measurement." },

            { EN + CAREGVR + MSG + REMINDER_IGNORED, "Your loved one ignored the reminder for blood pressure measurement." },
            { EN + CAREGVR + DES + REMINDER_IGNORED, "Please take action and call to remind them of the measurement." },

            { EN + CAREGVR + MSG + MEASUREMENT_IGNORED, "Your loved one did not take the blood pressure measurement." },
            { EN + CAREGVR + DES + MEASUREMENT_IGNORED, "Please take action and call to remind them of the measurement." },


            //WEIGHT ANALYSIS
            { EN + USR + MSG + WEIGHT_DEC, "There's a decrease of {0} kg in your weight." },
            { EN + USR + DES + WEIGHT_DEC, "Please take your meals regularly." },
            { EN + CAREGVR + MSG + WEIGHT_DEC, "Your loved one lost {0} kg." },
            { EN + CAREGVR + DES + WEIGHT_DEC, "You can contact him and see what's wrong." },

            { EN + USR + MSG + WEIGHT_INC, "There's a increase of {0} kg in your weight." },
            { EN + USR + DES + WEIGHT_INC, "Please be careful with your meals." },
            { EN + CAREGVR + MSG + WEIGHT_INC, "Your loved one gained {0} kg." },
            { EN + CAREGVR + DES + WEIGHT_INC, "Please check if this has to do with his diet." },


            //PULSE 
            { EN + USR + MSG + PULSE_LOW, "Hey! Your heart rate is quite low: {0}." },
            { EN + USR + DES + PULSE_LOW, "Please be careful with your meals." },
            { EN + CAREGVR + MSG + PULSE_LOW, "Your loved ones's heart rate is dangerously low: only {0}!" },
            { EN + CAREGVR + DES + PULSE_LOW, "Please take action now!" },

            { EN + USR + MSG + PULSE_MID_LOW, "Hey! Your heart rate is just a bit low: {0}." },
            { EN + USR + DES + PULSE_MID_LOW, "How about some exercise?" },
            { EN + CAREGVR + MSG + PULSE_MID_LOW, "Your loved ones's heart rate is a bit low: only {0}!" },
            { EN + CAREGVR + DES + PULSE_MID_LOW, "Please make sure they are all right." },

            { EN + USR + MSG + PULSE_MEDIUM, "Hey! Your heart rate is just a bit high:  {0}." },
            { EN + USR + DES + PULSE_MEDIUM, "Why not rest for a bit" },
            { EN + CAREGVR + MSG + PULSE_MEDIUM, "Your loved ones's heart rate is a bit high: over {0}!" },
            { EN + CAREGVR + DES + PULSE_MEDIUM, "Please make sure they are all right." },

            { EN + USR + MSG + PULSE_HIGH, "Hey! Your heart rate is just a quite high:  {0}." },
            { EN + USR + DES + PULSE_HIGH, "I have contacted your caregiver." },
            { EN + CAREGVR + MSG + PULSE_HIGH, "Your loved one's heart rate is dangerously high: over {0}!" },
            { EN + CAREGVR + DES + PULSE_HIGH, "Please take action now!" },

        };





        public static string Get(string lang, string type, string category, string who = "")
        {
            return text[lang + who + type + category];
        }
    }
}
