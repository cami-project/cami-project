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
        private readonly Action<BathroomVisitsWeek> bathoroomVisitsWeekHandler;

        public BathroomVisitService(Inform inform, Action<BathroomVisitsTwoDays> bathroomVisitsDayHandler, Action<BathroomVisitsWeek> bathoroomVisitsWeekHandler)
        {
            this.inform = inform;
            this.bathroomServiceHandler = bathroomVisitsDayHandler;
            this.bathoroomVisitsWeekHandler = bathoroomVisitsWeekHandler;
        }

        public void Check(int userID)
        {
            Console.WriteLine("Check bathroom visists invoked");

            var visits = inform.StoreAPI.GetBathroomVisitsForLastDays(30, userID);
            var groups = visits.GroupByDay();

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

            var daysOfWeek = new List<int>();

            if(dayBeforeYesterday.GetHashCode() != nullKeyValue.GetHashCode()){
                dayBeforeYesterdayCount = dayBeforeYesterday.Value.Count;
            }

            daysOfWeek.Add(dayBeforeYesterdayCount);

            for (var i = 1; i < 6; i++)
            {
                var day = groups.Find(x => x.Key == yesterdayDate.AddDays(-i - 1));

                if (day.GetHashCode() != nullKeyValue.GetHashCode())
                {
                    daysOfWeek.Add(day.Value.Count);
                }
                else 
                {
                    daysOfWeek.Add(-1);
                }
            }

            var twoDaysVisits = new BathroomVisitsTwoDays(yesterdayCount, dayBeforeYesterdayCount);
            var weekDaysVisits = new BathroomVisitsWeek(yesterdayCount, daysOfWeek); 


            // Console.WriteLine(twoDaysVisits);

            this.bathroomServiceHandler(twoDaysVisits);
            this.bathoroomVisitsWeekHandler(weekDaysVisits);
        }

        public void InformCaregiverDayDifference(BathroomVisitsTwoDays visits) {

            Console.WriteLine("Informing the caregiver: " + visits);
        }

        public void InformCaregiverWeekAvgAbnormal(BathroomVisitsWeek visits){

            Console.WriteLine("Informing the caregiver: " + visits);
        }
    }
}
