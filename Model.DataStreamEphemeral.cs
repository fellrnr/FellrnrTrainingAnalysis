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
        //Note: there is an instance of each DataStream object for each activity
        [MemoryPackConstructor]
        protected DataStreamEphemeral() { RequiredFields = new List<string>(); } //for use by memory pack deserialization only

        public DataStreamEphemeral(string name, List<string> requiredFields, Activity parent) : base(name, parent)
        {
            RequiredFields = requiredFields;
        }

        public override bool IsValid()
        {
            if (Parent == null) return false;

            foreach (string s in RequiredFields)
            {
                if (!Parent.TimeSeriesNames.Contains(s))
                    return false;
            }
            return true;
        }

        public override bool IsVirtual() { return true; }


        //we don't persist the cached data, but we hang onto it during the run
        [MemoryPackIgnore]
        Tuple<uint[], float[]>? CachedData = null;

        public abstract Tuple<uint[], float[]>? CalculateData();

        public override Tuple<uint[], float[]>? GetData()
        {
            if(CachedData == null)
                CachedData = CalculateData();
            return CachedData;
        }

        [MemoryPackInclude]
        protected List<string> RequiredFields { get; set; }

        public override void Recalculate(int forceCount, bool forceJustMe) { LastForceCount = forceCount; CachedData = null; return; }
    }
}
