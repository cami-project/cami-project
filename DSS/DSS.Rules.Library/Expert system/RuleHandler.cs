using System;
using Newtonsoft.Json;
using NRules;
using NRules.Fluent;
using DSS.RMQ;

namespace DSS.Rules.Library
{
    public class RuleHandler : IHandler
    {

        private JsonSerializerSettings settings;
        private ISessionFactory factory;

        private WeightService weightService;
        private PulseService pulseService;
        private StepsService stepsService;
        private ReminderService reminderService;
        private MotionService motionService;
        private ExerciseService exerciseService;
        private FallService fallService;
        private BloodPressureService bloodPressureService;
        private SuspiciousBehaviour suspiciousBehaviour;

        private ITimeService timeService;

        private BathroomVisitService bathroomVisitService;
      
        private readonly IInform inform;
        private readonly IActivityLog activityLog;

        public RuleHandler(IInform inform, IActivityLog activityLog)
        {
            this.inform = inform;
            this.activityLog = activityLog;

            settings = new JsonSerializerSettings();
            settings.Converters.Add(new MeasurementConverter());

            var repository = new RuleRepository();
            repository.Load(x => x.From(typeof(FallRule).Assembly));
            factory = repository.Compile();

            //var insertionURL = "http://cami-insertion:8010/api/v1/insertion";
            //var insertionURL = "http://141.85.241.224:8010/api/v1/insertion";
            //var storeURL = "http://cami-store:8008";
            //var storeURL = "http://141.85.241.224:8008";

            Console.WriteLine("REMEBER TO REPLACE THE MOCK STORE API");


            weightService = new WeightService(inform);
            pulseService = new PulseService(inform);
            stepsService = new StepsService(inform);
            reminderService = new ReminderService(inform);
            motionService = new MotionService(inform, this);
            exerciseService = new ExerciseService(inform);
            fallService = new FallService(inform);
            bloodPressureService = new BloodPressureService(inform);
            suspiciousBehaviour = new SuspiciousBehaviour(inform);

            bathroomVisitService = new BathroomVisitService(inform, BathroomVisitsDayHandler, BathroomVisitsWeekHandler);

            timeService = new MockTimeService(true, true);

        }


        public void Handle(object e)
        {
            var session = factory.CreateSession();
            session.Insert(reminderService);
            session.Insert(motionService);
            session.Insert(exerciseService);
            session.Insert(fallService);
            session.Insert(activityLog);
            session.Insert(weightService);
            session.Insert(bloodPressureService);
            session.Insert(e);
            //session.Insert(new MockActivityLogger());

            session.Fire();

        }


        public void HandleEvent(Event e)
        {
            Console.WriteLine("Event handler: " + e.content.name);

            var session = factory.CreateSession();
            session.Insert(reminderService);
            session.Insert(motionService);
            session.Insert(exerciseService);
            session.Insert(fallService);
            session.Insert(activityLog);
            //session.Insert(new MockActivityLogger());

            session.Insert(e);
            session.Fire();
        }

        public void HandleEventJSON(string json)
        {
            var obj = JsonConvert.DeserializeObject<Event>(json);


            if (obj != null)
            {
                var session = factory.CreateSession();
                session.Insert(reminderService);
                session.Insert(motionService);
                session.Insert(exerciseService);
                session.Insert(fallService);
                session.Insert(activityLog);

                if (obj.category == "USER_ENVIRONMENT")
                {

                    var objection = JsonConvert.DeserializeObject<MotionEvent>(json);


                    Console.WriteLine(objection.annotations.source.gateway + objection.annotations.source.sensor);

                    session.Insert(objection);

                    Console.WriteLine("U pitanju je: " + objection.getLocationName());
                }
                else
                {

                    session.Insert(obj);
                }

                session.Fire();
            }
        }

        public void HandleMeasurement(string json)
        {
            var obj = JsonConvert.DeserializeObject<Measurement>(json, settings);

            Console.WriteLine("Handle measurement");

            if (obj != null)
            {
                var session = factory.CreateSession();

                session.Insert(weightService);
                session.Insert(pulseService);
                session.Insert(bloodPressureService);

                session.Insert(obj);

                session.Fire();
            }
        }

        public void HandleSheduled(SheduledEvent obj)
        {
            if (obj != null)
            {
                var session = factory.CreateSession();

                session.Insert(activityLog);
                session.Insert(stepsService);
                session.Insert(fallService);
                session.Insert(motionService);
                session.Insert(suspiciousBehaviour);
                session.Insert(timeService);
                session.Insert(reminderService);


                session.Insert(obj);

                session.Fire();
            }
        }


        public void HandleLocationTimeSpent(LocationTimeSpent locationTimeSpent)
        {
            Console.WriteLine("[Handler - location time spent]: " + locationTimeSpent.ToString());

            try
            {
                var session = factory.CreateSession();

                session.Insert(suspiciousBehaviour);
                session.Insert(locationTimeSpent);

                session.Fire();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Handle location time spend NRULES exception: " + ex);
            }

        }

        public void HandleLocationChange(LocationChange locationChange)
        {

            Console.WriteLine("[Handler - location change]: " + locationChange.ToString());

            try
            {
                var session = factory.CreateSession();

                session.Insert(reminderService);
                session.Insert(locationChange);
                session.Insert(motionService);
                session.Insert(timeService);
                session.Insert(suspiciousBehaviour);
                session.Insert(activityLog);

                session.Fire();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Handle location change NRULES exception: " + ex);
            }
        }

        public void ActivityHandler(Activity activity)
        {
            Console.WriteLine("[Handler - activity]: " + activity.ToString());


            try
            {
                var session = factory.CreateSession();

                session.Insert(suspiciousBehaviour);
                session.Insert(fallService);
                session.Insert(activity);

                session.Fire();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Handle activity NRULES exception: " + ex);
            }

        }

        public void BathroomVisitsDayHandler(BathroomVisitsTwoDays visits)
        {

            Console.WriteLine("[Handler - Bathoroom visits per day]: " + visits.ToString());

            try
            {
                var session = factory.CreateSession();

                session.Insert(bathroomVisitService);
                session.Insert(visits);

                session.Fire();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Handle activity NRULES exception: " + ex);
            }
        }

        public void BathroomVisitsWeekHandler(BathroomVisitsWeek visits)
        {
            Console.WriteLine("[Handler - Bathoroom visits per week]: " + visits.ToString());

            try
            {
                var session = factory.CreateSession();

                session.Insert(bathroomVisitService);
                session.Insert(visits);

                session.Fire();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Handle activity NRULES exception: " + ex);
            }
        }
    
     }
}
