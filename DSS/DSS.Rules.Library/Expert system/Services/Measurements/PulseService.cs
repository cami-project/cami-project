using System;
using Newtonsoft.Json;

namespace DSS.Rules.Library
{
    public class PulseService : IRuleService<Measurement>
    {
        private readonly IInform inform;

        public PulseService(IInform inform)
        {
            this.inform = inform;

        }

        public void Save(Measurement measurement) {

            Console.WriteLine("Pushed to the db");

            measurement.ok = measurement.isPulseOK();
            inform.StoreAPI.PushMeasurement(JsonConvert.SerializeObject(measurement));
        }

        public void InformOfLowPulse(Measurement measurement) {


            Console.WriteLine("Low pulse");


            var LANG = inform.StoreAPI.GetLang(measurement.user);


            var endUserMsg = string.Format(Loc.Get(LANG, Loc.MSG, Loc.PULSE_LOW, Loc.USR), measurement.getValue());
            var caregiverMsg = string.Format(Loc.Get(LANG, Loc.MSG, Loc.PULSE_LOW, Loc.CAREGVR), measurement.getValue());

            inform.User(measurement.user, "heart", "high", endUserMsg, Loc.Get(LANG, Loc.DES, Loc.PULSE_LOW, Loc.USR));
            inform.Caregivers(measurement.user, "heart", "high", caregiverMsg, Loc.Get(LANG, Loc.DES, Loc.PULSE_LOW, Loc.CAREGVR));
        }

        public void InformOfMidLowPulse(Measurement measurement){


            Console.WriteLine("Mid pulse");
            return;

            var LANG = inform.StoreAPI.GetLang(measurement.user);


            var endUserMsg = string.Format(Loc.Get(LANG, Loc.MSG, Loc.PULSE_MID_LOW, Loc.USR), measurement.getValue());
            var caregiverMsg = string.Format(Loc.Get(LANG, Loc.MSG, Loc.PULSE_MID_LOW, Loc.CAREGVR), measurement.getValue());

            inform.User(measurement.user, "heart", "medium", endUserMsg, Loc.Get(LANG, Loc.DES, Loc.PULSE_MID_LOW, Loc.USR));
            inform.Caregivers(measurement.user, "heart", "medium", caregiverMsg, Loc.Get(LANG, Loc.DES, Loc.PULSE_MID_LOW, Loc.CAREGVR));
        }

        public void InformOfMidHighPulse(Measurement measurement){

            var LANG = inform.StoreAPI.GetLang(measurement.user);


            var endUserMsg = string.Format(Loc.Get(LANG, Loc.MSG, Loc.PULSE_MEDIUM, Loc.USR), measurement.getValue());
            var caregiverMsg = string.Format(Loc.Get(LANG, Loc.MSG, Loc.PULSE_MEDIUM, Loc.CAREGVR), measurement.getValue());

            inform.User(measurement.user, "heart", "medium", endUserMsg, Loc.Get(LANG, Loc.DES, Loc.PULSE_MEDIUM, Loc.USR));
            inform.Caregivers(measurement.user, "heart", "medium", caregiverMsg, Loc.Get(LANG, Loc.DES, Loc.PULSE_MEDIUM, Loc.CAREGVR));

        }

        public void InformOfHighPulse(Measurement measurement){

            var LANG = inform.StoreAPI.GetLang(measurement.user);


            var endUserMsg = string.Format(Loc.Get(LANG, Loc.MSG, Loc.PULSE_HIGH, Loc.USR), measurement.getValue());
            var caregiverMsg = string.Format(Loc.Get(LANG, Loc.MSG, Loc.PULSE_HIGH, Loc.CAREGVR), measurement.getValue());

            inform.User(measurement.user, "heart", "high", endUserMsg, Loc.Get(LANG, Loc.DES, Loc.PULSE_HIGH, Loc.USR));
            inform.Caregivers(measurement.user, "heart", "high", caregiverMsg, Loc.Get(LANG, Loc.DES, Loc.PULSE_HIGH, Loc.CAREGVR));
        }

        public void UpdateFact(Measurement val)
        {
            throw new NotImplementedException();
        }
    }
}
