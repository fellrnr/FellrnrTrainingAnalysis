using FellrnrTrainingAnalysis.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FellrnrTrainingAnalysis.Model
{
    public class DataStreamFactory
    {

        private DataStreamFactory()
        {
            DataStreams = new List<IDataStream>
            {
                new DataStreamToDataField("Pace", DataStreamToDataField.Mode.Average,
                    new DataStreamDelta("Calc.Pace", new List<string> { "Distance" })), //meters per second

                new DataStreamToDataField("Climb", DataStreamToDataField.Mode.MinMax,
                    new DataStreamDelta("Calc.Climb", new List<string> { "Altitude" }, 60.0f)), //meters per minute

                new DataStreamToDataField("Grade Adjusted Distance", DataStreamToDataField.Mode.LastValue, 
                        new DataStreamGradeAdjustedDistance("Grade Adjusted Distance", new List<string> {  "Distance", "Altitude" })),

                new DataStreamToDataField("Grade Adjusted Pace", DataStreamToDataField.Mode.Average,
                    new DataStreamDelta("Grade Adjusted Pace", new List<string> { "Grade Adjusted Distance" })), //meters per second
            };
        }
        static DataStreamFactory() { }
        public static DataStreamFactory Instance { get; set; } = new DataStreamFactory();

        public List<IDataStream> DataStreams { get; set; }

    }
}
