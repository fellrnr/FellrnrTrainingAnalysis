namespace FellrnrTrainingAnalysis.Model
{
    public interface ICalculate
    {

        //each activity calls the calculation recalculate
        public void Recalculate(Activity parent, bool force);
    }
}
