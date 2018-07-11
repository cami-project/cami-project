using Newtonsoft.Json;

namespace DSS.Rules.Library.JSON
{
    public partial class ReminderJson
    {
        public class Content
        {
            [JsonProperty("uuid")]
            public string Uuid { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("value_type")]
            public string ValueType { get; set; }

            [JsonProperty("value")]
            public Value Value { get; set; }
        }

    }
}
