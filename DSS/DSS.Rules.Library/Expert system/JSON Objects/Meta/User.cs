using Newtonsoft.Json;

namespace DSS.Rules.Library.JSON
{
    public partial class ReminderJson
    {
        public class User
        {
            [JsonProperty("id")]
            public long Id { get; set; }
        }

    }
}
