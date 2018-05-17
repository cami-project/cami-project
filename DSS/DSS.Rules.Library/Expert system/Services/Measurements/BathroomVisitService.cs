using System;
using System.Linq;
using System.Collections.Generic;
using DSS.RMQ;

namespace DSS.Rules.Library
{
    public class BathroomVisitService
    {
        private readonly Inform inform;
        private readonly Action<BathroomVisitsTwoDays> bathroomServiceHandler; 

        public BathroomVisitService(Inform inform, Action<BathroomVisitsTwoDays> bathroomVisitsDayHandler)
        {
            this.inform = inform;
            this.bathroomServiceHandler = bathroomVisitsDayHandler;
        }

        public void Get(int userID)
        {
            Console.WriteLine("Get bathroom visists invoked");

            var visits = inform.storeAPI.GetBathroomVisitsForLastDays(30, userID);
            var groups = visits.GroupByDay();

            //generate a system event 

            var yesterdayDate = DateTime.UtcNow.Date.AddDays(-1);
            var yesterday = groups.Find(x => x.Key == yesterdayDate);
            var dayBeforeYesterday = groups.Find(x => x.Key == yesterdayDate.AddDays(-1));

            var yesterdayCount = -1;
            var dayBeforeYesterdayCount = -1;

            var nullKeyValue = default(KeyValuePair<DateTime, JournalEntryResponse>);

            //Yesterday exist 
            if (yesterday.GetHashCode() != nullKeyValue.GetHashCode()){
                yesterdayCount = yesterday.Value.Count;
            }
            if(dayBeforeYesterday.GetHashCode() != nullKeyValue.GetHashCode()){
                dayBeforeYesterdayCount = dayBeforeYesterday.Value.Count;
            }

            var twoDaysVisits = new BathroomVisitsTwoDays(yesterdayCount, dayBeforeYesterdayCount);
           // Console.WriteLine(twoDaysVisits);

            this.bathroomServiceHandler(twoDaysVisits);
        }

        public void InformCaregiverDayDifference(BathroomVisitsTwoDays visits) {

            Console.WriteLine("Informing the caregiver: " + visits.ToString());

        }
    }
}
