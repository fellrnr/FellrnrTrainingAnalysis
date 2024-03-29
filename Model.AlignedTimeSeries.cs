using FellrnrTrainingAnalysis.Utils;
using pi.science.regression;
using System.Text;
using System.Xml.Linq;

namespace FellrnrTrainingAnalysis.Model
{
    public class AlignedTimeSeries
    {

        public static AlignedTimeSeries? Align(TimeValueList primary, TimeValueList secondary)
        {
            Logging.Instance.ContinueAccumulator("Utils.TimeSeries.AlignedTimeSeries");

            uint[] ptimes = primary.Times;
            float[] pvalues = primary.Values;
            uint[] stimes = secondary.Times;
            float[] svalues = secondary.Values;


            AlignedTimeSeries? aligned = null;
            if (ptimes.Length == stimes.Length)
            {
                //let's assume they match
                aligned = new AlignedTimeSeries(ptimes, pvalues, svalues);
            }
            else
            {
                List<uint> newTimes = new List<uint>();
                List<float> newPrimary = new List<float>();
                List<float> newSecondary = new List<float>();

                int si = 0;
                for (int pi = 0; pi < ptimes.Length && si < stimes.Length; pi++)
                {
                    while (pi < ptimes.Length && ptimes[pi] < stimes[si])
                    {
                        pi++;
                    }

                    while (si < stimes.Length && pi < ptimes.Length && ptimes[pi] > stimes[si]) //we incremented pi above, so could be over
                    {
                        si++;
                    }

                    if (si < stimes.Length && pi < ptimes.Length && ptimes[pi] == stimes[si]) // pi < ptimes.Length && si < stimes.Length
                    {
                        //all good
                        newTimes.Add(ptimes[pi]);
                        newPrimary.Add(pvalues[pi]);
                        newSecondary.Add(svalues[si]);
                        if (si < stimes.Length)
                            si++;
                    }
                }

                if (newTimes.Count != newPrimary.Count || newTimes.Count != newSecondary.Count)
                {
                    throw new Exception($"AlignedTimeSeries times {newTimes.Count} and primary {newPrimary.Count} secondary {newSecondary.Count} don't match counts");
                }


                aligned = new AlignedTimeSeries(newTimes.ToArray(), newPrimary.ToArray(), newSecondary.ToArray());
            }

            Logging.Instance.PauseAccumulator("Utils.TimeSeries.AlignedTimeSeries");
            return aligned;
        }

        private AlignedTimeSeries(uint[] time, float[] primary, float[] secondary)
        {
            Time = time;
            Primary = primary;
            Secondary = secondary;
        }

        public int Length { get { return Time.Length; } }

        public uint[] Time { get; set; }
        public float[] Primary { get; set; }
        public float[] Secondary { get; set; }


        public string ToCsv()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Time,Primary,Secondary");
            for (int i = 0; i < Length; i++)
            {
                sb.AppendLine($"{Time[i]},{Primary[i]},{Secondary[i]}");
            }
            return sb.ToString();
        }



    }
}
