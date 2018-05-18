namespace DSS.Rules.Library
{
    public class LocationTimeSpent
    {
        public string Name;
        public int Min;

        public string Owner;

        public LocationTimeSpent(string owner, string name, int min)
        {
            this.Owner = owner;
            this.Name = name;
            this.Min = min;
        }

        public override string ToString()
        {
            return Owner + " in " + Name + " for " + Min;
        }

        public bool Is(string name, int min)
        {
            return this.Name == name && this.Min == min;
        }
    }
}
