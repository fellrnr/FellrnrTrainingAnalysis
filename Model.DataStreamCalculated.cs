using MemoryPack;


namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]
    public partial class DataStreamCalculated : DataStreamEphemeral
    {
        [MemoryPackConstructor]
        protected DataStreamCalculated()  //for use by memory pack deserialization only
        {
        }
        public DataStreamCalculated(string name, List<string> requiredFields, Activity activity) : base(name, requiredFields, activity)
        {
        }


        public override Tuple<uint[], float[]> GetData() { return new Tuple<uint[], float[]>(new uint[1], new float[1]); }

        public override void Recalculate(bool force) { return; }

    }
}
