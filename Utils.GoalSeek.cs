using de.schumacher_bw.Strava.Endpoint;
using FellrnrTrainingAnalysis.Model;
using static FellrnrTrainingAnalysis.Model.TimeSeriesBase;
using System.Globalization;

namespace FellrnrTrainingAnalysis.Utils
{
    public class GoalSeek
    {
        public GoalSeek(Athlete athlete) 
        {
            Athlete = athlete;
        }
        Athlete Athlete { get; set; }


        const int high = 2;
        const int mid = 1;
        const int low = 0;

        public void GoalSeekPower()
        {
            //=POWER(A2,2)*15.14+A2*2.896+1.0098
            float[] x = new float[3];
            float[] x2 = new float[3];
            float[,] sds = new float[3,3];

            x2[high] = 30;
            x2[low] = 0;

            x[high] = 6;
            x[low] = 0;

            bool done = false;
            SortedDictionary<float, Tuple<int, int>> map = new SortedDictionary<float, Tuple<int, int>>();
            for(int  i = 0; i < 10 && !done; i++)
            {
                x2[mid] = (x2[high] + x2[low]) / 2;
                x[mid] = (x[high] + x[low]) / 2;

                for(int xi =low ; xi < high; xi++)
                {
                    for (int x2i = low; x2i < high; x2i++)
                    {
                        float sd = GoalSeekPower(x[xi], x2[x2i]);
                        sds[xi, x2i] = sd;
                        while (map.ContainsKey(sd))
                            sd += 0.000001f; //add a tiny bit
                        map.Add(sd, new Tuple<int, int>(xi, x2i));
                    }
                }

                //find the lowest pair
                Tuple<int, int> b1 = map[0];
                Tuple<int, int> b2 = map[1];



            }

        }
        public float GoalSeekPower(double x, double x2)
        {
            //compare recorded power with estimated power

            /*
            List<float> StandardDeviations = new List<float>();
            foreach (var kvp in Athlete.Activities)
            {
                Activity activity = kvp.Value;
                TimeSeriesBase? recordedPower = GetRecordedTimeSeries(activity, "Power");
                TimeSeriesBase? recordedDistance = GetRecordedTimeSeries(activity, "Distance");
                TimeSeriesBase? recordedAltitude = GetRecordedTimeSeries(activity, "Altitude");

                if (recordedPower != null && recordedDistance != null && recordedAltitude != null)
                {
                    //we have the data we need, so calculate power from distance and alitude
                    AlignedTimeSeries? alignedPowerAltitude = AlignedTimeSeries.Align(recordedDistance.GetData(0, false)!, recordedAltitude.GetData(0, false)!);

                    if (alignedPowerAltitude != null)
                    {
                        Utils.GradeAdjustedDistance gradeAdjustedDistance = new Utils.GradeAdjustedDistance(alignedPowerAltitude, (float)x2, (float)x);

                        TimeValueList gradeAdjustedPace = gradeAdjustedDistance.GetGradeAdjustedPace();

                        AlignedTimeSeries? alignedGapPower = AlignedTimeSeries.Align(recordedPower.GetData(0, false)!, gradeAdjustedPace);

                        if (alignedGapPower != null)
                        {
                            float sd = GetStandardDeviationOfError(activity, alignedGapPower);
                            StandardDeviations.Add(sd);
                        }
                    }
                }
            }

            float avgSD = StandardDeviations.Average();
            return avgSD;
            */
            return 0;
        }

        private float GetStandardDeviationOfError(Activity activity, AlignedTimeSeries alignedGapPower)
        {
            float weight = Athlete.FindDailyValueOrDefault((DateTime)activity.StartDateNoTimeLocal!, Model.Day.TagWeight, Options.Instance.StartingWeight);

            float[] power = alignedGapPower.Primary;
            float[] gap = alignedGapPower.Secondary;
            float[] powerPerKg = new float[power.Length]; //running power is roughly the same as pace in m/s, though greater for better running econnomy
            float[] powerError = new float[power.Length];

            for (int i = 0; i < powerPerKg.Length; i++)
            {
                powerPerKg[i] = power[i] / weight;
                float diff = powerPerKg[i] - gap[i];
                powerError[i] = diff;
            }

            float average = powerError.Average();

            float sum = 0;
            foreach (float f in powerError)
            {
                float diff = f - average;
                sum += diff * diff;
            }
            float sd = (float)Math.Sqrt(sum / powerError.Length);
            return sd;
        }

        private TimeSeriesBase? GetRecordedTimeSeries(Activity activity, string name) 
        {
            if (activity.TimeSeries.ContainsKey(name) && !activity.TimeSeries[name].IsVirtual())
                return activity.TimeSeries[name];
            else 
                return null;
        }
    }

}
