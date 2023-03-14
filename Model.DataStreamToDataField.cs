using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FellrnrTrainingAnalysis.Model
{
    //a simple shim class that fits wraps another DataStreamEphemeral and puts some data on the Activity based on the wrapped class
    [Serializable]
    public class DataStreamToDataField : IDataStream
    {
        public DataStreamToDataField(string activityFieldname, Mode extractionMode, IDataStream dataStream) 
        {
            Name = dataStream.Name; //we pretend to be the wrapped dataStream
            DataStream = dataStream;
            ExtractionMode = extractionMode;
            ActivityFieldname = activityFieldname;
        }

        IDataStream DataStream { get; set; }

        public enum Mode { LastValue, Average }
        Mode ExtractionMode { get; set; }

        public Tuple<uint[], float[]>? GetData(Activity parent)
        {
            return DataStream.GetData(parent);
        }

        public bool IsValid(Activity parent)
        {
            return DataStream.IsValid(parent);
        }

        public string Name { get; set; }

        public string ActivityFieldname { get; set; }


        public void Recalculate(Activity parent, bool force)
        {
            DataStream.Recalculate(parent, force);

            if (parent.HasNamedDatum(ActivityFieldname))
                return;

            Tuple<uint[], float[]>? data = GetData(parent);
            if (data == null) { return; }

            float value;
            if(ExtractionMode == Mode.LastValue)
            {
                value = data.Item2.Last();
            }
            else
            {
                value = data.Item2.Average();
            }
            parent.AddOrReplaceDatum(new TypedDatum<float>(ActivityFieldname, false, value));
        }



    }
}
