using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSS.Rules.Library.JSON
{
    public partial class ReminderJson
    {
        public class Journal
        {
            [JsonProperty("id_enduser")]
            public long IdEnduser { get; set; }

            [JsonProperty("id_caregivers")]
            public List<long> IdCaregivers { get; set; }
        }

    }
}
