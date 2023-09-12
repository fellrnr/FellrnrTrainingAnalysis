namespace FellrnrTrainingAnalysis.Model
{
    [Serializable]
    public abstract class DataStreamEphemeral : IDataStream
    {
        public DataStreamEphemeral(string name, List<string> requiredFields)
        {
            RequiredFields = requiredFields;
            Name = name;
        }

        public bool IsValid(Activity parent)
        {
            foreach (string s in RequiredFields)
            {
                if (!parent.TimeSeriesNames.Contains(s))
                    return false;
            }
            return true;
        }

        public bool IsVirtual() { return true; }

        public abstract Tuple<uint[], float[]>? GetData(Activity parent);

        protected List<string> RequiredFields { get; }

        public string Name { get; }

        public abstract void Recalculate(Activity parent, bool force);

    }
}
