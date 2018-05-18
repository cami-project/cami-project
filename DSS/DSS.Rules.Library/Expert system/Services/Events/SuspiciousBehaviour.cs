using System;
namespace DSS.Rules.Library
{
    public class SuspiciousBehaviour
    {
        private IInform inform;

        public SuspiciousBehaviour(IInform inform)
        {
            this.inform = inform;
        }

        public void ToLongInBathroom(LocationTimeSpent locationTime)
        {
            Console.WriteLine("Too long in the bathroom");
        }


        public void NightWandering(Activity  activity) 
        {
            Console.WriteLine("Night wandering");
        }
    }
}
