using FellrnrTrainingAnalysis.Utils;
using MemoryPack;
using System.Windows.Forms;

namespace FellrnrTrainingAnalysis.Model
{
    public abstract class CalculateFieldBase
    {

        [MemoryPackIgnore]
        protected int LastForceCount = 0;


        //each activity calls the calculation recalculate
        public abstract void Recalculate(Extensible extensible, int forceCount, bool forceJustMe);

    }
}
