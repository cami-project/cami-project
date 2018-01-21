using System;
namespace DSS.Rules.Library
{
    public class ExerciseService
    {
        readonly Inform inform;

        public ExerciseService(Inform inform)
        {
            this.inform = inform;
        }

        public void InformCaregiver(Event exerciseEvent) {


            var key = exerciseEvent.getUserURI()+ "_exercise";
            var LANG = inform.storeAPI.GetLang(exerciseEvent.getUserURI());


            if (exerciseEvent.content.name == "exercise_started")
            {
                
                Console.WriteLine("[ReminderHandler] Exercise started");
                Console.WriteLine("[ReminderHandler] userActiveExcercise map key: " + key);

                InMemoryDB.Push(key, exerciseEvent);

                string exerciseType = exerciseEvent.content.val.exercise_type.ToString();

                inform.Caregivers(exerciseEvent.getUserURI(), "exercise", "none", Loc.Get(LANG, Loc.MSG, Loc.EXERCISE_STARTED, Loc.CAREGVR), string.Format(Loc.Get(LANG, Loc.DES, Loc.EXERCISE_STARTED, Loc.CAREGVR), exerciseType));
            }
            else if (exerciseEvent.content.name == "exercise_ended")
            {
                Console.WriteLine("[ReminderHandler] Exercise ended");
                Console.WriteLine("[ReminderHandler] userActiveExcercise map key: " + key);

                if (InMemoryDB.Exists(key) && exerciseEvent.content.val.session_uuid == InMemoryDB.Get<Event>(key).content.val.session_uuid)
                {

                    Console.WriteLine("Exercise ended is in the active map");

                    float score = float.Parse(exerciseEvent.content.val.score.ToString());
                    string exerciseType = exerciseEvent.content.val.exercise_type.ToString();

                    inform.Caregivers(exerciseEvent.getUserURI(),
                                     "exercise",
                                     "none",
                                     Loc.Get(LANG, Loc.MSG, Loc.EXERCISE_ENDED, Loc.CAREGVR),
                                     string.Format(Loc.Get(LANG, Loc.DES, Loc.EXERCISE_ENDED, Loc.CAREGVR), exerciseType, score));



                    var desc = Loc.EXERCISE_ENDED_HIGH;

                    if (score <= 30)
                        desc = Loc.EXERCISE_ENDED_LOW;
                    else if (score > 30 && score <= 60)
                        desc = Loc.EXERCISE_ENDED_MID;


                    inform.User(exerciseEvent.getUserURI(), "exercise", "low", Loc.Get(LANG, Loc.MSG, Loc.EXERCISE_ENDED_LOW, Loc.USR), Loc.Get(LANG, Loc.DES, desc, Loc.USR));
                    InMemoryDB.Remove(key);
                }
            }
        }
    }
}
