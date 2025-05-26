using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FellrnrTrainingAnalysis.Model;

namespace FellrnrTrainingAnalysis.Action
{
    public abstract class StressBalanceModeller
    {
        public StressBalanceModeller()
        {
        }

        public class StressData
        {
            public StressData(float atl, float ctl, float tsb)
            {
                ATL = atl;
                CTL = ctl;
                TSB = tsb;
            }

            public float ATL { get; set; }
            public float CTL { get; set; }
            public float TSB { get; set; }
        }

        public abstract StressData Calculate(StressData? previous, Model.Day day);

    }

    public class StressBalanceModellerSimple : StressBalanceModeller
    {
        public StressBalanceModellerSimple(string stressField, float dATL, float dCTL)
        {
            StressField = stressField;
            DaysCTL = dCTL;
            DaysATL = dATL;
            lambdaATL = 2.0f / (1.0f + DaysATL);
            lambdaCTL = 2.0f / (1.0f + DaysCTL);
        }

        private string StressField { get; set; }
        private float DaysATL { get; set; }
        private float DaysCTL { get; set; }

        float lambdaATL;
        float lambdaCTL;

        public override StressData Calculate(StressData? previous, Model.Day day)
        {
            if(previous == null)
                previous = new StressData(0, 0, 0);

            float? thisDaysStressNull = day.GetNamedFloatDatum(StressField);
            if (thisDaysStressNull == null)
                thisDaysStressNull = 0;
            float thisDaysStress = thisDaysStressNull.Value;

            float ctl = thisDaysStress * lambdaCTL + ((1 - lambdaCTL) * previous.CTL);
            float atl = thisDaysStress * lambdaATL + ((1 - lambdaATL) * previous.ATL);

            float tsb = ctl - atl;

            StressData next = new StressData(atl: atl, ctl: ctl, tsb: tsb);

            return next;
        }

    }

}
