using System;
namespace DSS.Rules.Library
{
    public class SuspiciousBehaviour
    {
        private Inform inform;

        public SuspiciousBehaviour(Inform inform)
        {
            this.inform = inform;
        }

        public void ToLongInBathroom(LocationTimeSpent locationTime)
        {
            Console.WriteLine("Too long in the bathroom");
        }
    }
}
