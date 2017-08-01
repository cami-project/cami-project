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

        }

        public string Do(Event e){
            
            return mapTable[e.content.name];

        }
    }
}
