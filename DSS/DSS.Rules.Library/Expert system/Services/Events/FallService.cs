using System;
namespace DSS.Rules.Library
{
    public class FallService
    {
        private readonly Inform inform;

        public FallService(Inform inform)
        {
            this.inform = inform;
        }

        public void InformCaregiver(Event fall)
        {
            Console.WriteLine("FALL INFORM");

            var gatewayURIPath = fall.annotations.source["gateway"].ToString();
            var userPath = inform.StoreAPI.GetUserOfGateway(gatewayURIPath);
            var lang = inform.StoreAPI.GetLang(userPath);


            inform.Caregivers(userPath, "fall", "high",
                              Loc.Msg(Loc.FALL, lang, Loc.CAREGVR),
                              Loc.Des(Loc.FALL, lang, Loc.CAREGVR));
            
        }

        public void InformCaregiverOfMovementAfterFall()
        {


            Console.WriteLine("Informing caregiver of movement after a fall");
        }
    }
}
