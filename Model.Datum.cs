namespace FellrnrTrainingAnalysis.Model
{
    [Serializable]
    public class Datum
    {
        public Datum(string name, bool recorded)
        {
            Name = name;
            Recorded = recorded;
        }
        public string Name { get; set; }

        public bool Recorded { get; set; }
    }

    [Serializable]
    public class TypedDatum<T> : Datum
    {
        public TypedDatum(string name, bool recorded, T data) : base(name, recorded)
        {
            Data = data;
        }
        /// Override ToString() for display
        public override string ToString()
        {
            return Data == null ? "null" : Data.ToString()!;
        }

        public T Data { get; set; }
    }

}