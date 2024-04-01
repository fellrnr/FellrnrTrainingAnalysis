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

            AlignedTimeSeries? aligned = null;
            if (primary.Length == secondary.Length)
            {
                //let's assume they match
                aligned = new AlignedTimeSeries(primary.Values, secondary.Values);
            }
            else
            {
                float[] p;
                float[] s;
                if(primary.Length < secondary.Length)
                {
                    p = primary.Values;
                    s = secondary.Values[..(p.Length)];
                }
                else
                {
                    s = secondary.Values;
                    p = primary.Values[..(s.Length)];
                }

                aligned = new AlignedTimeSeries(p, s);
            }

            Logging.Instance.PauseAccumulator("Utils.TimeSeries.AlignedTimeSeries");
            return aligned;
        }

        private AlignedTimeSeries(float[] primary, float[] secondary)
        {
            Primary = primary;
            Secondary = secondary;
        }

        public int Length { get { return Primary.Length; } }

        public float[] Primary { get; set; }
        public float[] Secondary { get; set; }


        public string ToCsv()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Time,Primary,Secondary");
            for (int i = 0; i < Length; i++)
            {
                sb.AppendLine($"{i},{Primary[i]},{Secondary[i]}");
            }
            return sb.ToString();
        }



    }
}
