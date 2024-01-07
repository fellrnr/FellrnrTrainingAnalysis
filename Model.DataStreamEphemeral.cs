using MemoryPack;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]
    [MemoryPackUnion(0, typeof(DataStreamDelta))]
    [MemoryPackUnion(1, typeof(DataStreamCalculated))]
    [MemoryPackUnion(2, typeof(DataStreamGradeAdjustedDistance))]
    public abstract partial class DataStreamEphemeral : DataStreamBase
    {
        [MemoryPackConstructor]
        protected DataStreamEphemeral()  //for use by memory pack deserialization only
        {
        }

        public DataStreamEphemeral(string name, List<string> requiredFields, Activity parent) : base(name, parent)
        {
            RequiredFields = requiredFields;
        }

        public override bool IsValid()
        {
            foreach (string s in RequiredFields)
            {
                if (!Parent.TimeSeriesNames.Contains(s))
                    return false;
            }
            return true;
        }

        public override bool IsVirtual() { return true; }

        //public abstract Tuple<uint[], float[]>? GetData(Activity parent);

        [MemoryPackInclude]
        protected List<string> RequiredFields { get; }

        //public string Name { get; }

        //public abstract void Recalculate(Activity parent, bool force);

    }
}
