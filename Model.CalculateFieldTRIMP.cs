using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FellrnrTrainingAnalysis.Model
{
    public abstract class CalculateFieldTRIMP : ICalculateField
    {
        protected CalculateFieldTRIMP(string fieldSubname)
        {
            FieldSubname = fieldSubname;
        }

        //fieldname is from "TRIMP" + the subname
        private string FieldSubname { get; }

        private const string FieldTRIMP = "TRIMP";

        private string FieldName {  get { return FieldTRIMP + FieldSubname;  } }

        public void Recalculate(Extensible extensible, bool force)
        {
            if(extensible.HasNamedDatum(FieldName) && !force) { return; }


        }
    }
}