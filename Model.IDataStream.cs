
namespace FellrnrTrainingAnalysis.Model
{
    public interface IDataStream
    {
        Tuple<uint[], float[]>? GetData(Activity parent);

        bool IsValid(Activity parent);

        string Name { get; }

        void Recalculate(Activity parent, bool force);

    }
}