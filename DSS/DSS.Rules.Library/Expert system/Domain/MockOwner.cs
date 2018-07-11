using System.Collections.Generic;

namespace DSS.Rules.Library
{
    public class MockOwner : IOwner
    {
        public MockOwner(string owner, string lang = "EN", string timezone = "EN")
        {
            this.Owner = owner;
            this.Lang = lang;
            this.Timezone = timezone;
        }

        public string Owner { get; set; }
        public string Lang { get; set; }
        public string Timezone { get; set; }
        public IList<string> Caregivers { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    }
}
