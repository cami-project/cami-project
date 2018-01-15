using System;
using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class ExerciseStartRule: Rule
    {
        ExerciseService service = null;
        Event exercise = null;

        public override void Define()
        {
            When().Exists<Event>(reminder => exercise.isExerciseStarted())
                  .Match(() => service);

            Then().Do(ctx => service.InformCaregiver(exercise));
        }
    }
}
