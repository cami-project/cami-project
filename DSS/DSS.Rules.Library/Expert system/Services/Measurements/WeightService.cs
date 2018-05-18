using System;
using Newtonsoft.Json;

namespace DSS.Rules.Library
{
    public class WeightService : IRuleService<Measurement>
    {

        private Inform inform;

        public WeightService(Inform inform)
        {
            this.inform = inform;
        }

        public float GetLatestWeight(string enduserURI)
        {
            return inform.StoreAPI.GetLatestWeightMeasurement(inform.GetIdFromURI(enduserURI));
        }

        //private void InformUser(string trend, float differenceInKG, string enduserURI, string LANG)
        //{

        //    var category = trend == "up" ? Loc.WEIGHT_INC : Loc.WEIGHT_DEC;
        //    var endUserMsg = string.Format(Loc.Get(LANG, Loc.MSG, category, Loc.USR), differenceInKG);
        //    inform.User(enduserURI, "weight", "medium", endUserMsg, Loc.Get(LANG, Loc.DES, category, Loc.USR));
        //}
        //private void InformCaregiver(string trend, float differenceInKG, string enduserURI, string LANG)
        //{
            
        //    var category = trend == "up" ? Loc.WEIGHT_INC : Loc.WEIGHT_DEC;
        //    var caregiverMsg = string.Format(Loc.Get(LANG, Loc.MSG, category, Loc.CAREGVR), differenceInKG);
        //    inform.Caregivers(enduserURI, "weight", "medium", caregiverMsg, Loc.Get(LANG, Loc.DES, category, Loc.CAREGVR));
        //}


        public void Inform(string trend, float differenceInKG, string enduserURI, string LANG){

            var category = trend == "up" ? Loc.WEIGHT_INC : Loc.WEIGHT_DEC;
            var endUserMsg = string.Format(Loc.Get(LANG, Loc.MSG, category, Loc.USR), differenceInKG);
            var caregiverMsg = string.Format(Loc.Get(LANG, Loc.MSG, category, Loc.CAREGVR), differenceInKG);
			
            inform.User(enduserURI, "weight", "medium", endUserMsg, Loc.Get(LANG, Loc.DES, category, Loc.USR));
            inform.Caregivers(enduserURI, "weight", "medium", caregiverMsg, Loc.Get(LANG, Loc.DES, category, Loc.CAREGVR));

        }

        public bool IsBiggerThenPreviousBy(Measurement measure,float kg) {

            return measure.differentIsBigger(GetLatestWeight(measure.user), kg);
        }

        public bool IsLessThenPreviousBy(Measurement measure, float kg){
            
            return measure.differenceIsLess(GetLatestWeight(measure.user), kg);
        }


        public void WeightIncrease(Measurement measure){

            var differenceInKg = measure.differenceInKg(inform.StoreAPI.GetLatestWeightMeasurement(inform.GetIdFromURI(measure.user)));
            Inform("up", differenceInKg, measure.user, inform.StoreAPI.GetLang(measure.user));

        }
        public void WeightDrop(Measurement measure){

            var differenceInKg = measure.differenceInKg(inform.StoreAPI.GetLatestWeightMeasurement(inform.GetIdFromURI(measure.user)));
            Inform("down", differenceInKg, measure.user, inform.StoreAPI.GetLang(measure.user));
        }

        public void Save(Measurement measure)
        {
            inform.StoreAPI.PushMeasurement(JsonConvert.SerializeObject( measure));
        }

        public void UpdateFact(Measurement val)
        {
            throw new NotImplementedException();
        }
    }
}
