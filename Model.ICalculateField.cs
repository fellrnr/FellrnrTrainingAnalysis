namespace FellrnrTrainingAnalysis.Model
{
    public interface ICalculateField
    {

        //each activity calls the calculation recalculate
        public void Recalculate(Extensible extensible, bool force);
    }
}
