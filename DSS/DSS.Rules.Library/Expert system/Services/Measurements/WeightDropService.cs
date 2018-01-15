using System;
namespace DSS.Rules.Library
{

    //TODO: Check if this makes sense
    public class WeightDropService : IRuleService<Measurement>
    {
        Inform inform;



        public WeightDropService(Inform inform){

            this.inform = inform;
        }

        public void InformCaregiver()
        {
            
        }

  

        public void UpdateFact(Measurement val)
        {
            throw new NotImplementedException();
        }
    }
}
