using FellrnrTrainingAnalysis.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FellrnrTrainingAnalysis.UI
{
    public class DatumFormatter
    {
        public static string FormatForGrid(Datum datum, ActivityDatumMetadata activityDatumMetadata)
        {
            if (datum == null) { return "";  }
            if(activityDatumMetadata == null) { return datum.ToString(); }

            switch(activityDatumMetadata.DataType)
            {
                case ActivityDatumMetadata.DataTypeEnum.Float:
                case ActivityDatumMetadata.DataTypeEnum.String:
                case ActivityDatumMetadata.DataTypeEnum.DateTime:
            }


        }

    }
}
