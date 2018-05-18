namespace DSS.Rules.Library
{
    public class LocationChange
    {
        public string Current;
        public string Previous;

        public string ID;

        public LocationChange(string userID, string previous, string current)
        {
            this.ID = userID;
            this.Current = current;
            this.Previous = previous;
        }

        public bool FromTo(string from, string to)
        {
            return from == this.Previous && to == this.Current;
        }


        public override string ToString()
        {
            return "From " + this.Previous + " to " + this.Current;
        }
    }
}
