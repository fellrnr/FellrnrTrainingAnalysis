namespace FellrnrTrainingAnalysis.Model
{
    public interface IDataStream
    {

        //need parent in case we need to get other data to calculate this stream
        Tuple<uint[], float[]>? GetData(Activity parent);

        bool IsValid(Activity parent);

        bool IsVirtual();

        string Name { get; }

        void Recalculate(Activity parent, bool force);



    }
}