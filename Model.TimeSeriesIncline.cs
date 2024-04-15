using FellrnrTrainingAnalysis.Utils;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]

    //A class that calculates grade adjusted distance from horizontal distance and elevation changes
    public partial class TimeSeriesIncline : TimeSeriesEphemeral
    {
        private const string SPAN = "SpanPeriod";

        [MemoryPackConstructor]
        protected TimeSeriesIncline()  //for use by memory pack deserialization only
        {
        }

        public TimeSeriesIncline(string name,
                                    Activity parent,
                                    bool persistCache,
                                    List<string>? requiredFields,
                                    List<string>? opposingFields = null,
                                    List<string>? sportsToInclude = null,
                                    int spanPeriod = 60) :
            base(name, parent, persistCache, requiredFields, opposingFields, sportsToInclude)
        {
            Parameter(SPAN, spanPeriod);
        }

        public override TimeValueList? CalculateData(int forceCount, bool forceJustMe)
        {
            if (forceJustMe) Logging.Instance.TraceEntry($"TimeSeriesIncline - Forced recalculating");

            TimeSeriesBase distanceStream = RequiredTimeSeries[0];
            TimeValueList? distanceData = distanceStream.GetData(forceCount, forceJustMe);
            if (distanceData == null || distanceData.Length < 1)
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"No distance");
                return null;
            }


            //altitude data is quite granular, with changes occuring periodically
            //Therefore we need to use a span of time
            TimeSeriesBase altitudeStream = RequiredTimeSeries[1];
            TimeValueList? altitudeData = altitudeStream.GetData(forceCount, forceJustMe);
            if (altitudeData == null || altitudeData.Length < 1)
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"No altitude");
                return null;
            }

            int finalTimeAltitude = altitudeData.Length;
            int finalTimeDistance = distanceData.Length;
            int finalTimeDistance90 = finalTimeDistance * 90 / 100;
            if (finalTimeAltitude < finalTimeDistance90)
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"altitude less than 90 distance, {finalTimeAltitude}, {finalTimeDistance}, {finalTimeDistance90}");
                return null;
            }

            int spanPeriod = (int)ParameterOrNull(SPAN)!;
            float scalingFactor = 1.0f / (float)ParameterOrNull(SPAN)!;

            //float[] distanceDeltas = CalculateDelta(distanceData.Values);

            //need to span over 60 seconds for distance as well as altitude
            TimeValueList distanceDeltas = TimeValueList.SpanDeltas(tvl: distanceData,
                                                                     scalingFactor: scalingFactor, //scale to average the delta over 60 seconds
                                                                     numerator: null,
                                                                     limit: null,
                                                                     period: spanPeriod,
                                                                     extraDebug: forceJustMe);


            TimeValueList altitudeDeltas = TimeValueList.SpanDeltas(tvl: altitudeData,
                                                                     scalingFactor: scalingFactor, //scale to average the delta over 60 seconds
                                                                     numerator: null,
                                                                     limit: null,
                                                                     period: spanPeriod,
                                                                     extraDebug: forceJustMe);


            //calcualte grade from distance and altitude deltas
            float[] rawGrades = CalculateGrade(distanceDeltas.Values, altitudeDeltas.Values);



            TimeValueList retval = new TimeValueList(rawGrades);


            if (forceJustMe) Logging.Instance.TraceLeave($"return Incline {retval}");
            return retval;
        }

        private float[] CalculateGrade(float[] distanceDeltas, float[] altitudeDeltas)
        {

            float[] grades = new float[distanceDeltas.Length];

            grades[0] = 0; //no previous value, grade of the first point has to be zero
            int length = Math.Min(distanceDeltas.Length, altitudeDeltas.Length);
            for (int i = 1; i < length; i++) //note starting from one, as the first element is zero
            {
                float distanceDelta = distanceDeltas[i];
                float altitudeDelta = altitudeDeltas[i];
                float grade;
                if (distanceDelta == 0)
                {
                    grade = 0;
                }
                else
                {
                    grade = altitudeDelta / distanceDelta;

                    //constrain slope before smoothing in case of discontinuities
                    if (grade > Options.Instance.MaxSlope)
                        grade = (float)Options.Instance.MaxSlope;

                    if (grade < Options.Instance.MinSlope)
                        grade = (float)Options.Instance.MinSlope;
                }
                grades[i] = grade;
                if (grade != 0 && !float.IsNormal(grade))
                {
                    Logging.Instance.Log(string.Format("invalid grade value {0} from altitude {1} and distance {2} at offset {3}", grade, altitudeDelta, distanceDelta, i));
                }
            }
            return grades;
        }


    }
}
