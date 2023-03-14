
namespace FellrnrTrainingAnalysis.Model
{
    [Serializable]
    public class DataStreamCalculated : DataStreamEphemeral
    {
        public DataStreamCalculated(string name, List<string> requiredFields) : base(name, requiredFields)
        {
        }


        public override Tuple<uint[], float[]> GetData(Activity Parent) { return new Tuple<uint[], float[]>(new uint[1], new float[1]); }

        public override void Recalculate(Activity parent, bool force) { return; }

    }
}
