using FellrnrTrainingAnalysis.Utils;
using GMap.NET.MapProviders;
using MemoryPack;
using System.Collections.Generic;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]

    //from https://markliversedge.blogspot.com/2014/07/wbal-its-implementation-and-optimisation.html
    //And Golden Chetah
    // This code implements the W replenishment / utilisation algorithm
    // as defined in "Modeling the Expenditure and Reconstitution of Work Capacity 
    // above Critical Power." Med Sci Sports Exerc 2012;:1.
    public partial class TimeSeriesWPrimeBalance : TimeSeriesEphemeral
    {
        [MemoryPackConstructor]
        protected TimeSeriesWPrimeBalance()  //for use by memory pack deserialization only
        {
        }

        public TimeSeriesWPrimeBalance(string name, Activity parent, bool persistCache, List<string>? requiredFields, List<string>? opposingFields = null, List<string>? sportsToInclude = null) :
            base(name, parent, persistCache, requiredFields, opposingFields, sportsToInclude)
        {
        }

        private const string WPRIME = "WPrime";
        [MemoryPackIgnore]
        private float WPrime { get { return ParameterOrZero(WPRIME); } set { Parameter(WPRIME, value); } }

        private const string CRITICALPOWER = "CriticalPower";
        [MemoryPackIgnore]
        private float CriticalPower { get { return ParameterOrZero(CRITICALPOWER); } set { Parameter(CRITICALPOWER, value); } }

        const double WprimeMultConst = 1.0;
        const int WPrimeDecayPeriod = 1800; // 1 hour, tried infinite but costly and limited value
                                            //         on long rides anyway
        const double E = 2.71828183;

        public override TimeValueList? CalculateData(int forceCount, bool forceJustMe)
        {
            if (forceJustMe) Logging.Instance.TraceEntry($"TimeSeriesWPrime - Forced recalculating {this.Name}");

            TimeSeriesBase powerStream = RequiredTimeSeries[0];
            TimeValueList? powerData = powerStream.GetData(forceCount, forceJustMe);
            if (powerData == null || powerData.Length < 1)
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"No power");
                return null;
            }

            Athlete athlete = ParentActivity!.ParentAthlete!;
            if (WPrime == 0)
            {
                WPrime = athlete.FindDailyValueOrDefault((DateTime)ParentActivity!.StartDateNoTimeLocal!, Day.TagWPrime, Options.Instance.StartingWPrime);
            }
            if (CriticalPower== 0)
            {
                CriticalPower = athlete.FindDailyValueOrDefault((DateTime)ParentActivity!.StartDateNoTimeLocal!, Day.TagCriticalPower, Options.Instance.StartingCriticalPower);
            }

            if(WPrime < 1000)
                WPrime *= 1000;

            float[] powerArray = powerData.Values;
            float[] wPrimeBalanceValues = new float[powerData.Length];
            float CP = CriticalPower;


            //We're implementing differential
            //From https://groups.google.com/g/golden-cheetah-users/c/fPcfvyHLTMc?pli=1
            // I prefer differential formula at present because tau is dynamic with respect to recovery power whereas tau for the integral model
            // is static based on the entire ride duration.  Therefore, lets say you do a short unders/overs type bout for 10 min inside of
            // a longer 2-3 hr ride at Z2.  The static tau for the integral model will grossly underestimate the actual value that
            // should occur during the unders/overs segement.  

            float W = WPrime;
            for (int i = 0; i < powerData.Length; i++)
            {
                float pwr = powerArray[i];
                if (pwr < CP)
                {
                    W = W + (CP - pwr) * (WPrime - W) / WPrime;
                }
                else
                {
                    W = W + (CP - pwr);
                }

                if(float.IsNaN(W))
                    W = 0;
                wPrimeBalanceValues[i] = W;
            }

            TimeValueList retval = new TimeValueList(wPrimeBalanceValues);

            if (forceJustMe) Logging.Instance.TraceLeave($"Done");

            return retval;
        }
    }
}
