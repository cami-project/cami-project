using System;
using System.Collections.Generic;
using AForge.Fuzzy;
using DSS.Delegate;
using System.Linq;

namespace DSS.FuzzyInference
{
    public class FuzzyContainer
    {

		private InferenceSystem fis;
        private EventToLabel e2l;


		public FuzzyContainer()
        {

			var HR = new LinguisticVariable("HR", 0, 100);
			new List<FuzzySet>()
			{
				new FuzzySet("Low", new TrapezoidalFunction(0, 5, 25, 30)),
				new FuzzySet("Medium", new TrapezoidalFunction(20, 25, 55, 60)),
				new FuzzySet("High", new TrapezoidalFunction(50, 55, 95 ,100))
			}.ForEach(x => HR.AddLabel(x));

			//Output
			var heartRate = new LinguisticVariable("HEART_RATE", 0, 100);
			new List<FuzzySet>()
			{
				new FuzzySet("Low", new TrapezoidalFunction(0, 5, 25, 30)),
				new FuzzySet("Medium", new TrapezoidalFunction(20, 25, 55, 60)),
				new FuzzySet("High", new TrapezoidalFunction(50, 55, 95 ,100))
			}.ForEach(x => heartRate.AddLabel(x));

			var db = new Database();
			db.AddVariable(HR);
			db.AddVariable(heartRate);

			fis = new InferenceSystem(db, new CentroidDefuzzifier(1000));

            fis.NewRule("Rule 1", "IF HR IS Low THEN HEART_RATE IS Low");
			fis.NewRule("Rule 2", "IF HR IS Medium THEN HEART_RATE IS Medium");
			fis.NewRule("Rule 3", "IF HR IS High THEN HEART_RATE IS High");



            e2l = new EventToLabel();
        }

        public string Infer(List<Event> events){


            foreach (var e in events)
            {
				fis.SetInput(e2l.Do(e), e.content.val.numVal);
			}

            var executed = fis.ExecuteInference("HEART_RATE");

            if (executed == null || executed.OutputList.Count == 0)
                return "";

            return executed.OutputVariable.Name  + "-" + executed.OutputList[0].Label;
		}
    }
}
