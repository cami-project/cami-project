using System;
using System.Collections.Generic;
using AForge.Fuzzy;

namespace DSS.FuzzyInference
{
    public class Wearable
    {
        
		private InferenceSystem fisI;
		private InferenceSystem fisII;


		public Wearable()
		{
			SetUpFisI();
			SetUpFisII();
		}

		private void SetUpFisI()
		{
			//Input
			var treshhold = new LinguisticVariable("Threshold", 0, 100);
			new List<FuzzySet>()
			{
				new FuzzySet("Low", new TrapezoidalFunction(0, 5, 25, 30)),
				new FuzzySet("Medium", new TrapezoidalFunction(20, 25, 55, 60)),
				new FuzzySet("High", new TrapezoidalFunction(50, 55, 95 ,100))
			}.ForEach(x => treshhold.AddLabel(x));

			//Output
			var impact = new LinguisticVariable("Impact", 0, 100);
			new List<FuzzySet>()
			{
				new FuzzySet("Low", new TrapezoidalFunction(0, 5, 25, 30)),
				new FuzzySet("Medium", new TrapezoidalFunction(20, 25, 55, 60)),
				new FuzzySet("High", new TrapezoidalFunction(50, 55, 95 ,100))
			}.ForEach(x => impact.AddLabel(x));

			var db = new Database();
			db.AddVariable(treshhold);
			db.AddVariable(impact);


			fisI = new InferenceSystem(db, new CentroidDefuzzifier(1000));

			fisI.NewRule("Rule 1", "IF Threshold IS Low THEN Impact IS Low");
			fisI.NewRule("Rule 2", "IF Threshold IS Medium THEN Impact IS Medium");
			fisI.NewRule("Rule 3", "IF Threshold IS High THEN Impact IS High");

		}

		public void EvaluateFisI(float threshold)
		{

			fisI.SetInput("Threshold", threshold);

			//  Console.WriteLine("Impact : " + fisI.ExecuteInference("Impact").OutputList[0].Label);
		}

		private void SetUpFisII()
		{

			//Input
			var impact = new LinguisticVariable("Impact", 0, 100);
			new List<FuzzySet>()
			{
				new FuzzySet("Low", new TrapezoidalFunction(0, 5, 25, 30)),
				new FuzzySet("Medium", new TrapezoidalFunction(20, 25, 55, 60)),
				new FuzzySet("High", new TrapezoidalFunction(50, 55, 95 ,100))
			}.ForEach(x => impact.AddLabel(x));


			var onGround = new LinguisticVariable("On_ground", -0.5f, 1.5f);
			new List<FuzzySet>()
			{
				new FuzzySet("Low", new TrapezoidalFunction(-0.5f, - 0.2f, 0.2f, 0.5f)),
				new FuzzySet("Medium", new TrapezoidalFunction(0.1f, 0.5f,0.5f,0.9f)),
				new FuzzySet("High", new TrapezoidalFunction(0.5f ,0.8f,1.2f,1.5f))
			}.ForEach(x => onGround.AddLabel(x));


			var timeOnGround = new LinguisticVariable("Time_on_ground", -1, 901);
			new List<FuzzySet>()
			{
				new FuzzySet("Brief", new TrapezoidalFunction(-1, 1, 1, 2)),
				new FuzzySet("Short", new TrapezoidalFunction(1, 5, 10, 15)),
				new FuzzySet("Moderate", new TrapezoidalFunction(10, 120, 480, 720)),
				new FuzzySet("Long", new TrapezoidalFunction(480, 900, 900, 901))

			}.ForEach(x => timeOnGround.AddLabel(x));


			//Output
			var fallWearable = new LinguisticVariable("Fall_wearable", -0.5f, 1.5f);
			new List<FuzzySet>()
			{
				new FuzzySet("Low", new TrapezoidalFunction( -0.5f, -0.2f, 0.2f, 0.5f)),
				new FuzzySet("Medium", new TrapezoidalFunction(0.1f, 0.5f, 0.5f, 0.9f)),
				new FuzzySet("High", new TrapezoidalFunction(0.5f, 0.8f, 1.2f, 1.5f))
			}.ForEach(x => fallWearable.AddLabel(x));

			var db = new Database();

			db.AddVariable(impact);
			db.AddVariable(onGround);
			db.AddVariable(timeOnGround);
			db.AddVariable(fallWearable);


			fisII = new InferenceSystem(db, new CentroidDefuzzifier(1000));

			fisII.NewRule("Rule 1", "IF Impact IS High AND On_ground IS High AND Time_on_ground IS Long THEN Fall_wearable IS High");
			fisII.NewRule("Rule 2", "IF Impact IS Medium AND On_ground IS High AND Time_on_ground IS Long THEN Fall_wearable IS High");
			fisII.NewRule("Rule 3", "IF Impact IS Low AND On_ground IS Low AND (Time_on_ground IS Short OR Time_on_ground IS Brief) THEN Fall_wearable IS Low");
			fisII.NewRule("Rule 4", "IF Impact IS Medium AND On_ground IS Medium AND Time_on_ground IS Moderate THEN Fall_wearable IS Medium");
			fisII.NewRule("Rule 5", "IF Impact IS High AND On_ground IS Medium AND (Time_on_ground IS Short OR Time_on_ground IS Brief) THEN Fall_wearable IS Medium");


		}

		public void EvaluateFisII(float impact, float onGround, float timeOnGround)
		{

			fisII.SetInput("Impact", impact);
			fisII.SetInput("On_ground", onGround);
			fisII.SetInput("Time_on_ground", timeOnGround);

			//  Console.WriteLine("Fall wearable : " + fisII.ExecuteInference("Fall_wearable").OutputList[0].Label);


		}

        public string Execute(float treshold, float onGround, float timeOnGround)
		{

			fisI.SetInput("Threshold", treshold);

			var tresholdResult = fisII.ExecuteInference("Impact");

			var impact = fisI.Evaluate("Impact");

			fisII.SetInput("Impact", impact);
			fisII.SetInput("On_ground", onGround);
			fisII.SetInput("Time_on_ground", timeOnGround);

			var wearableResult = fisII.ExecuteInference("Fall_wearable");

            return string.Format("{0} - {1} - {2}",wearableResult.OutputList[0].Label, "Fall_wearable", fisII.Evaluate("Fall_wearable"));
		}
	}
    }

