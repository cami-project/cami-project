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
        private List<string> registredVariables;


		public FuzzyContainer()
        {

            registredVariables = new List<string>();

			//var HR = new LinguisticVariable("HR", 0, 200);
			//new List<FuzzySet>()
			//{
			//	new FuzzySet("Low", new TrapezoidalFunction(0, 5, 45, 50)),
			//	new FuzzySet("Medium", new TrapezoidalFunction(50, 55, 115, 120)),
			//	new FuzzySet("High", new TrapezoidalFunction(120, 125, 195 ,200))
			//}.ForEach(x => HR.AddLabel(x));


			
			var impact = new LinguisticVariable("IMPACT", 0, 100);
			new List<FuzzySet>()
			{
				new FuzzySet("Low", new TrapezoidalFunction(0, 5, 25, 30)),
				new FuzzySet("Medium", new TrapezoidalFunction(20, 25, 55, 60)),
				new FuzzySet("High", new TrapezoidalFunction(50, 55, 95 ,100))
			}.ForEach(x => impact.AddLabel(x));
            registredVariables.Add(impact.Name);


            Console.WriteLine("IMPACT name: " + impact.Name);


			var onGround = new LinguisticVariable("ON_GROUND", -0.5f, 1.5f);
			new List<FuzzySet>()
			{
				new FuzzySet("Low", new TrapezoidalFunction(-0.5f, - 0.2f, 0.2f, 0.5f)),
				new FuzzySet("Medium", new TrapezoidalFunction(0.1f, 0.5f,0.5f,0.9f)),
				new FuzzySet("High", new TrapezoidalFunction(0.5f ,0.8f,1.2f,1.5f))
			}.ForEach(x => onGround.AddLabel(x));
            registredVariables.Add(onGround.Name);

			var timeOnGround = new LinguisticVariable("TIME_ON_GROUND", -1, 901);
			new List<FuzzySet>()
			{
				new FuzzySet("Brief", new TrapezoidalFunction(-1, 1, 1, 2)),
				new FuzzySet("Short", new TrapezoidalFunction(1, 5, 10, 15)),
				new FuzzySet("Moderate", new TrapezoidalFunction(10, 120, 480, 720)),
				new FuzzySet("Long", new TrapezoidalFunction(480, 900, 900, 901))

			}.ForEach(x => timeOnGround.AddLabel(x));
            registredVariables.Add(timeOnGround.Name);


			//Output
			var fallWearable = new LinguisticVariable("FALL", -0.5f, 1.5f);
			new List<FuzzySet>()
			{
				new FuzzySet("Low", new TrapezoidalFunction( -0.5f, -0.2f, 0.2f, 0.5f)),
				new FuzzySet("Medium", new TrapezoidalFunction(0.1f, 0.5f, 0.5f, 0.9f)),
				new FuzzySet("High", new TrapezoidalFunction(0.5f, 0.8f, 1.2f, 1.5f))
			}.ForEach(x => fallWearable.AddLabel(x));


			//var heartRate = new LinguisticVariable("HEART_RATE", 0, 100);
			//new List<FuzzySet>()
			//{
			//	new FuzzySet("Low", new TrapezoidalFunction(0, 5, 25, 30)),
			//	new FuzzySet("Medium", new TrapezoidalFunction(20, 25, 55, 60)),
			//	new FuzzySet("High", new TrapezoidalFunction(50, 55, 95 ,100))
			//}.ForEach(x => heartRate.AddLabel(x));


			var db = new Database();
			//db.AddVariable(HR);
			//db.AddVariable(heartRate);

			db.AddVariable(impact);
			db.AddVariable(onGround);
			db.AddVariable(timeOnGround);
			db.AddVariable(fallWearable);


			fis = new InferenceSystem(db, new CentroidDefuzzifier(1000));

   //         fis.NewRule("Rule 1", "IF HR IS Low THEN HEART_RATE IS Low");
			//fis.NewRule("Rule 2", "IF HR IS Medium THEN HEART_RATE IS Medium");
			//fis.NewRule("Rule 3", "IF HR IS High THEN HEART_RATE IS High");


			fis.NewRule("Rule 4", "IF IMPACT IS High AND ON_GROUND IS High AND TIME_ON_GROUND IS Long THEN FALL IS High");
			fis.NewRule("Rule 5", "IF IMPACT IS Medium AND ON_GROUND IS High AND TIME_ON_GROUND IS Long THEN FALL IS High");
			fis.NewRule("Rule 6", "IF IMPACT IS Low AND ON_GROUND IS Low AND (TIME_ON_GROUND IS Short OR TIME_ON_GROUND IS Brief) THEN FALL IS Low");
			fis.NewRule("Rule 7", "IF IMPACT IS Medium AND ON_GROUND IS Medium AND TIME_ON_GROUND IS Moderate THEN FALL IS Medium");
			fis.NewRule("Rule 8", "IF IMPACT IS High AND ON_GROUND IS Medium AND (TIME_ON_GROUND IS Short OR TIME_ON_GROUND IS Brief) THEN FALL IS Medium");



			e2l = new EventToLabel();
        }

        public List<string> Infer(List<Event> events){


            Console.WriteLine("Inference: " + events.Count);

            var results = new List<string>();

            foreach (var e in events)
            {
                if(registredVariables.Contains( e2l.Do(e))) {

					fis.SetInput(e2l.Do(e), e.content.val.numVal);
                    Console.WriteLine(e2l.Do(e) +" - "+ e.content.val.numVal);
                }
			}

            //Execute("HEART_RATE", results);
			Execute("FALL", results);

			return results;
		}

        private void Execute(string name, List<string> results){
            
            var executed = fis.ExecuteInference(name);

			if (executed != null && executed.OutputList.Count != 0)
				results.Add(executed.OutputVariable.Name + "-" + executed.OutputList[0].Label);


		}
    }
}
