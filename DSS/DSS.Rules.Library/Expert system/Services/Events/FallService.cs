using System;
namespace DSS.Rules.Library
{
    public class FallService
    {
        private readonly IInform inform;

        public FallService(IInform inform)
        {
            this.inform = inform;
        }

        public void InformCaregiver(Event fall)
        {
            Console.WriteLine("FALL INFORM");


            //TODO: Enich the domain model at the entering point of the system 
            //var gatewayURIPath = fall.annotations.source["gateway"].ToString();
            //var userPath = inform.StoreAPI.GetUserOfGateway(gatewayURIPath);
            //var lang = inform.StoreAPI.GetLang(userPath);


            inform.Caregivers(fall.Owner, "fall", "high",
                              Loc.Msg(Loc.FALL, fall.Lang, Loc.CAREGVR),
                              Loc.Des(Loc.FALL, fall.Lang, Loc.CAREGVR));
            
            inform.ActivityLog.Log(new Activity(fall.Owner, ActivityType.Fall, "Fall happened", "FallService.InformCaregiver"));
                       
        }

        public void SheduleCheckForMovementAfter(string owner, int min)
        {
            Console.WriteLine("Shedule check for movement after: " + min);
            SheduleService.Add(new SheduledEvent(owner, SheduleService.Type.CheckMovementAfterFall, DateTime.UtcNow.AddMinutes(min)));
        }

        public void InformCaregiverOfMovementAfterFall()
        {
            Console.WriteLine("Informing caregiver of movement after a fall");
        }
    }
}
