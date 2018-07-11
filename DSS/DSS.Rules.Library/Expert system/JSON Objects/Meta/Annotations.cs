using Newtonsoft.Json;

namespace DSS.Rules.Library.JSON
{
    public partial class ReminderJson
    {
        public class Annotations
        {
            [JsonProperty("timestamp")]
            public long Timestamp { get; set; }

            [JsonProperty("source")]
            public string Source { get; set; }
        }

    }
}
