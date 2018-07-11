using System;
using System.Collections.Generic;

namespace DSS.Rules.Library.Domain
{
    public class ReminderEvent : IEvent
    {
        public string Type { get; set; }
        public string Uuid { get; set; }

        public DateTime Timestamp { get; set; }
        public string Owner { get; set;}
        public string Lang { get; set; }
        public string Timezone { get; set; }
        public IList<string> Caregivers { get; set; }


        public bool IsReminderSent()
        {
            return Type == "reminder_sent";
        }

        public bool IsReminderSnoozed()
        {
            return Type == "reminder_snoozed";
        }

        public bool IsReminderAcknowedged()
        {
            return Type == "reminder_acknowledged";
        }
    }
}
