using System.Collections.ObjectModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FellrnrTrainingAnalysis.Model
{
    [Serializable]

    //A class that calculates grade adjusted distance from horizontal distance and elevation changes
    public class DataStreamGradeAdjustedDistance : DataStreamEphemeral
    {
        public DataStreamGradeAdjustedDistance(string name, List<string> requiredFields) : base(name, requiredFields)
        {
            if (requiredFields.Count != 2) throw new ArgumentException("DataStreamDelta must have two required fields");
        }


        public override Tuple<uint[], float[]>? GetData(Activity parent)
        {
            ReadOnlyDictionary<string, IDataStream> timeSeries = parent.TimeSeries;
            string distanceField = RequiredFields[0];
            IDataStream distanceStream = timeSeries[distanceField];
            Tuple<uint[], float[]>? distanceData = distanceStream.GetData(parent);
            if (distanceData == null) { return null; }

            string altitudeField = RequiredFields[1];
            IDataStream altitudeStream = timeSeries[altitudeField];
            Tuple<uint[], float[]>? altitudeData = altitudeStream.GetData(parent);
            if (altitudeData == null) { return null; }

            if (distanceData.Item1.Length != altitudeData.Item1.Length)
            {
                Logging.Instance.Error(string.Format("GradeAdjustedDistance needs distance and altitude the same length for activity {0} from {1} for data stream {2} ", parent.PrimaryKey(), parent.StartDateTime, Name));
                return distanceData; //default to returning the raw distance data, otherwise the data isn't counted at all. 
            }
            Utils.GradeAdjustedDistance gradeAdjustedDistance = new Utils.GradeAdjustedDistance(distanceData, altitudeData);

            return gradeAdjustedDistance.GetGradeAdjustedDistance();

        }

        public override void Recalculate(Activity parent, bool force) { return; }

    }
}
