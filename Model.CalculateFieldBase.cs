using MemoryPack;

namespace FellrnrTrainingAnalysis.Model
{
    public abstract class CalculateFieldBase
    {

        [MemoryPackIgnore]
        protected int LastForceCount = 0;


        //each activity calls the calculation recalculate
        public abstract void Recalculate(Extensible extensible, int forceCount, bool forceJustMe);

        public void Recalculate(Extensible extensible, bool forceJustMe)
        {
            Recalculate(extensible, LastForceCount, forceJustMe);
        }
    }
}
