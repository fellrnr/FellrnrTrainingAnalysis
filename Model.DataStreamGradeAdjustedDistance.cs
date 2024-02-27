using MemoryPack;
using System.Collections.ObjectModel;
using FellrnrTrainingAnalysis.Utils;
using static FellrnrTrainingAnalysis.Utils.TimeSeries;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]

    //A class that calculates grade adjusted distance from horizontal distance and elevation changes
    public partial class DataStreamGradeAdjustedDistance : DataStreamEphemeral
    {
        [MemoryPackConstructor]
        protected DataStreamGradeAdjustedDistance()  //for use by memory pack deserialization only
        {
        }
        public DataStreamGradeAdjustedDistance(string name, List<string> requiredFields, Activity activity) : base(name, requiredFields, activity)
        {
            if (requiredFields.Count != 2) throw new ArgumentException("DataStreamDelta must have two required fields");
        }


        public override Tuple<uint[], float[]>? CalculateData()
        {
            if(Parent == null) {  return null; }

            ReadOnlyDictionary<string, DataStreamBase> timeSeries = Parent.TimeSeries;
            string distanceField = RequiredFields[0];
            DataStreamBase distanceStream = timeSeries[distanceField];
            Tuple<uint[], float[]>? distanceData = distanceStream.GetData();
            if (distanceData == null) { return null; }

            string altitudeField = RequiredFields[1];
            DataStreamBase altitudeStream = timeSeries[altitudeField];
            Tuple<uint[], float[]>? altitudeData = altitudeStream.GetData();
            if (altitudeData == null) 
            { 
                return distanceData; 
            }

            AlignedTimeSeries? aligned = TimeSeries.Align(distanceData, altitudeData);

            if (aligned == null)
            {
                return distanceData;
            }


            Utils.GradeAdjustedDistance gradeAdjustedDistance = new Utils.GradeAdjustedDistance(aligned);

            return gradeAdjustedDistance.GetGradeAdjustedDistance();


            /*
            if (distanceData.Item1.Length != altitudeData.Item1.Length)
            {
                //it's fairly common to have altitude data miss the first one or two data points
                bool matchable = true;
                int difference = distanceData.Item1.Length - altitudeData.Item1.Length;
                if (difference > 0 && difference <= 100 && difference * 10 < altitudeData.Item1.Length) //less than 50 points missing and we have 90+% of the data
                {
                    for (int i = difference; i < distanceData.Item1.Length && matchable; i++)
                    {
                        if (distanceData.Item1[i] != altitudeData.Item1[i - difference])
                        {
                            matchable = false;
                        }
                    }
                }
                else
                {
                    matchable = false;
                }

                if (matchable)
                {
                    Tuple<uint[], float[]>? distanceDataTrimmed = new Tuple<uint[], float[]>(distanceData.Item1.Skip(difference).ToArray(), distanceData.Item2.Skip(difference).ToArray());

                    Utils.GradeAdjustedDistance gradeAdjustedDistance = new Utils.GradeAdjustedDistance(distanceDataTrimmed, altitudeData);

                    return gradeAdjustedDistance.GetGradeAdjustedDistance();
                }
                else
                {
                    Logging.Instance.Error($"GradeAdjustedDistance: distance {distanceData.Item1.Length} points and altitude {altitudeData.Item1.Length} points for activity {Parent}for data");
                    return distanceData; //default to returning the raw distance data, otherwise the data isn't counted at all. 
                }
            }
            else
            {
                Utils.GradeAdjustedDistance gradeAdjustedDistance = new Utils.GradeAdjustedDistance(distanceData, altitudeData);

                return gradeAdjustedDistance.GetGradeAdjustedDistance();
            }
            */


        }



    }
}
