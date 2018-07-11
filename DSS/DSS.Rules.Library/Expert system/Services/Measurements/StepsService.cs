﻿using System;
namespace DSS.Rules.Library
{
    public class StepsService
    {
        readonly IInform inform;


        public StepsService(IInform inform)
        {
            this.inform = inform;

        }
        public void InformOfLowSteps(string userURIPath)
        {
            var stepsCount = GetStepsCount(userURIPath);
            var LANG = inform.StoreAPI.GetLang(userURIPath);

            var endUserMsg = Loc.Msg(Loc.STEPS_LESS_1000, LANG) ;
            var caregiverMsg = Loc.Msg(Loc.STEPS_LESS_1000, LANG);

            inform.User(userURIPath, "steps", "medium", endUserMsg, Loc.Des(Loc.STEPS_LESS_1000, LANG));
            inform.Caregivers(userURIPath, "steps", "medium", caregiverMsg, Loc.Des(Loc.STEPS_LESS_1000, LANG));
        }

        public int GetStepsCount(string userURIPath)
        {
            
            var now = DateTime.UtcNow;

            var startTs = (long)ChangeTime(now, 0, 0).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            var endTs = (long)now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds; ;


            var val = inform.StoreAPI.GetUserStepCount(userURIPath, startTs, endTs);

            Console.WriteLine("VAL: " +  val);

            return inform.StoreAPI.GetUserStepCount(userURIPath, startTs, endTs);
           
        }

        public bool stepsCountLessThan(int val, string usrURI) {

            return GetStepsCount(usrURI) < val;
        }

        public bool stepsCountBetween(int min, int max, string userURI) {

            var val = GetStepsCount(userURI);

            return val >= min && val <= max;
        }

        public bool stepsCountBiggerThan(int val, string usrURI) {
            
            return GetStepsCount(usrURI) > val;

        }

        private DateTime ChangeTime(DateTime date, int hour, int min)
        {
            return date.Date + new TimeSpan(hour, min, 0);
        }

        public void InformOfMediumSteps(string userURIPath)
        {

            var stepsCount = GetStepsCount(userURIPath);
            var LANG = inform.StoreAPI.GetLang(userURIPath);

            var endUserMsg = string.Format(Loc.Get(LANG, Loc.MSG, Loc.STEPS_BETWEEN_1000_2000, Loc.USR), stepsCount);
            var caregiverMsg = string.Format(Loc.Get(LANG, Loc.MSG, Loc.STEPS_BETWEEN_1000_2000, Loc.CAREGVR), stepsCount);

            inform.User(userURIPath, "steps", "low", endUserMsg, Loc.Get(LANG, Loc.DES, Loc.STEPS_BETWEEN_1000_2000, Loc.USR));
            inform.Caregivers(userURIPath, "steps", "low", caregiverMsg, Loc.Get(LANG, Loc.DES, Loc.STEPS_BETWEEN_1000_2000, Loc.CAREGVR));
        }

        public void InformHighSteps(string userURIPath)
        {

            var stepsCount = GetStepsCount(userURIPath);
            var LANG = inform.StoreAPI.GetLang(userURIPath);

            var endUserMsg = string.Format(Loc.Get(LANG, Loc.MSG, Loc.STEPS_BIGGER_6000, Loc.USR), stepsCount);
            var caregiverMsg = string.Format(Loc.Get(LANG, Loc.MSG, Loc.STEPS_BIGGER_6000, Loc.CAREGVR), stepsCount);

            inform.User(userURIPath, "steps", "low", endUserMsg, Loc.Get(LANG, Loc.DES, Loc.STEPS_BIGGER_6000, Loc.USR));
            inform.Caregivers(userURIPath, "steps", "low", caregiverMsg, Loc.Get(LANG, Loc.DES, Loc.STEPS_BIGGER_6000, Loc.CAREGVR));
        }



    }
}