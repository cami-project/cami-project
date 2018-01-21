﻿using System;
using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class ExerciseEndRule: Rule
    {
        ExerciseService service = null;
        Event exercise = null;

        public override void Define()
        {
            When().Exists<Event>(exercise => exercise.isExerciseEnded())
                  .Match(() => exercise)
                  .Match(() => service);

            Then().Do(ctx => service.InformCaregiver(exercise));

        }
    }
}
