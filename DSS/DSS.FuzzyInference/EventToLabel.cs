using System;
using System.Collections.Generic;
using DSS.Delegate;

namespace DSS.FuzzyInference
{
    public class EventToLabel
    {
        private Dictionary<string, string> mapTable;

        public EventToLabel()
        {
            mapTable = new Dictionary<string, string>();

            mapTable["Heart-Rate"] = "HR";
            mapTable["IMPACT"] = "IMPACT";
            mapTable["ON_GROUND"] = "ON_GROUND";
            mapTable["TIME_ON_GROUND"] = "TIME_ON_GROUND";
        }

        public string Do(Event e){
            
            return mapTable[e.content.name];

        }
    }
}
