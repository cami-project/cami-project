// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickType;
//
//    var reminderJson = ReminderJson.FromJson(jsonString);

namespace DSS.Rules.Library.JSON.Reminder
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class ReminderJson
    {
        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("content")]
        public Content Content { get; set; }

        [JsonProperty("annotations")]
        public Annotations Annotations { get; set; }
    }

    public partial class Annotations
    {
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }
    }

    public partial class Content
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

    public partial class Value
    {
        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("journal")]
        public Journal Journal { get; set; }
    }

    public partial class Journal
    {
        [JsonProperty("id_enduser")]
        public long IdEnduser { get; set; }

        [JsonProperty("id_caregivers")]
        public List<long> IdCaregivers { get; set; }
    }

    public partial class User
    {
        [JsonProperty("id")]
        public long Id { get; set; }
    }

    public partial class ReminderJson
    {
        public static ReminderJson FromJson(string json) => JsonConvert.DeserializeObject<ReminderJson>(json, Converter.Settings);


        public Domain.ReminderEvent ToDomain()
        {

            var path = "/api/v1/user/{0}/";

            var e = new Domain.ReminderEvent()
            {
                Type = this.Content.Name,
                Uuid = this.Content.Uuid
            };

            e.Owner = string.Format(path, this.Content.Value.User.Id);
            e.Caregivers = new List<string>();

            foreach (var caregiver in e.Caregivers)
            {
                e.Caregivers.Add(string.Format(path, caregiver));
            }

            return e;
        }
    }

    public static class Serialize
    {
        public static string ToJson(this ReminderJson self) => JsonConvert.SerializeObject(self, Converter.Settings);
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
