using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Linq;


using JournalEntryGroup = System.Collections.Generic.KeyValuePair<System.DateTime, System.Collections.Generic.List<DSS.RMQ.JournalEntryItem>>;


namespace DSS.RMQ
{

    
    public partial class JournalEntryResponse
    {
        [JsonProperty("meta")]
        public Meta Meta { get; set; }

        [JsonProperty("objects")]
        public List<JournalEntryItem> Objects { get; set; }
    }

    public class Meta
    {
        [JsonProperty("limit")]
        public long Limit { get; set; }

        [JsonProperty("next")]
        public object Next { get; set; }

        [JsonProperty("offset")]
        public long Offset { get; set; }

        [JsonProperty("previous")]
        public object Previous { get; set; }

        [JsonProperty("total_count")]
        public long TotalCount { get; set; }
    }

    public class JournalEntryItem
    {
        [JsonProperty("acknowledged")]
        public object Acknowledged { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("resource_uri")]
        public string ResourceUri { get; set; }

        [JsonProperty("severity")]
        public string Severity { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }


        public DateTime GetDate()
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return dtDateTime.AddSeconds(int.Parse(this.Timestamp)).Date;
        }

        public override string ToString()
        {
            return string.Format("{0} - {1} - {2} - {3}", Message, Description, Type, GetDate());
        }
    }


    public partial class JournalEntryResponse
    {
        public static JournalEntryResponse FromJson(string json) => JsonConvert.DeserializeObject<JournalEntryResponse>(json, Converter.Settings);

        public List<JournalEntryGroup> GroupByDay()
        {
            var grouped = from entry in Objects
                          group entry by entry.GetDate() into newGroup
                          select newGroup;
            
            var results = new List<JournalEntryGroup>();

            foreach (var dateGroup in grouped)
            {
                var keyVal = new JournalEntryGroup(dateGroup.Key, dateGroup.ToList());
                results.Add(keyVal);
            }

            Console.WriteLine("Grouped values: ");
            results.ForEach(x =>
            {
                Console.WriteLine(x.Key);
                x.Value.ForEach(y =>
                {
                    Console.WriteLine( y);
                });

            });
            return results;
        }
    }

    public static class Serialize
    {
        public static string ToJson(this JournalEntryResponse self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }


}
