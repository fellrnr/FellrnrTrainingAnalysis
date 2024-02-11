using FellrnrTrainingAnalysis.Model;

namespace FellrnrTrainingAnalysis.UI
{
    public class DatumFormatter
    {

        public static string FormatForGrid(Datum? datum, ActivityDatumMetadata activityDatumMetadata)
        {
            string retval = Format(datum, activityDatumMetadata);
            if(Utils.Options.Instance.DebugAddRawDataToGrids)
            {
                if (datum == null) 
                {
                    retval += " dnull";
                    //retval += " (datum null)"; 
                }
                else if(activityDatumMetadata == null)
                {
                    retval += " mnull";
                    //retval += " (meta null)";
                }
                else
                {
                    retval = $"Raw:{datum}/F:{retval}";
                    //retval += $" raw:{datum}";
                    //retval += $" [{datum}]";
                }
            }
            //retval += " tst";
            return retval;
        }

        public static string FormatForTree(Datum? datum, ActivityDatumMetadata activityDatumMetadata)
        {
            return FormatForGrid(datum, activityDatumMetadata);
        }

        private static string Format(Datum? datum, ActivityDatumMetadata activityDatumMetadata)
        {
            if (datum == null) { return "";  }
            if(activityDatumMetadata == null) { return datum.ToString()!; }

            float value = 0;

            switch (activityDatumMetadata.DisplayUnits)
            {
                case ActivityDatumMetadata.DisplayUnitsType.Meters:
                case ActivityDatumMetadata.DisplayUnitsType.Kilometers:
                case ActivityDatumMetadata.DisplayUnitsType.Pace:
                case ActivityDatumMetadata.DisplayUnitsType.TimeSpan:
                case ActivityDatumMetadata.DisplayUnitsType.Integer:
                case ActivityDatumMetadata.DisplayUnitsType.Percent:
                    if (datum is not TypedDatum<float>) { return ""; }

                    TypedDatum<float> floatDatum = (TypedDatum<float>)datum;
                    value = floatDatum.Data;
                    break;
                case ActivityDatumMetadata.DisplayUnitsType.None:
                case ActivityDatumMetadata.DisplayUnitsType.BPM:
                    break;
                default:
                    return "";
            }


            switch (activityDatumMetadata.DisplayUnits)
            {
                case ActivityDatumMetadata.DisplayUnitsType.Meters:
                    return Utils.Misc.FormatFloat(value, "{0:#,0} m", 1.0f);
                case ActivityDatumMetadata.DisplayUnitsType.Kilometers:
                    return Utils.Misc.FormatFloat(value, "{0:#,0.0} Km", 1.0f / 1000.0f);
                case ActivityDatumMetadata.DisplayUnitsType.Pace:
                    return Utils.Misc.FormatPace(value);
                case ActivityDatumMetadata.DisplayUnitsType.TimeSpan:
                    return Utils.Misc.FormatTime(value);
                case ActivityDatumMetadata.DisplayUnitsType.Integer:
                    return Utils.Misc.FormatFloat(value, "{0:#,0}", 1.0f);
                case ActivityDatumMetadata.DisplayUnitsType.Percent:
                    return Utils.Misc.FormatFloat(value, "{0:#,0}%", 1.0f);
                case ActivityDatumMetadata.DisplayUnitsType.None:
                case ActivityDatumMetadata.DisplayUnitsType.BPM:
                    return datum.ToString()!;
                default:
                    return "";
            }
        }


        public static bool RightJustify(ActivityDatumMetadata activityDatumMetadata) //no datum as we're doing this for the column, not the cell
        {
            if (activityDatumMetadata == null) { return false; }

            switch (activityDatumMetadata.DisplayUnits)
            {
                case ActivityDatumMetadata.DisplayUnitsType.Meters:
                case ActivityDatumMetadata.DisplayUnitsType.Kilometers:
                case ActivityDatumMetadata.DisplayUnitsType.Pace:
                case ActivityDatumMetadata.DisplayUnitsType.TimeSpan:
                case ActivityDatumMetadata.DisplayUnitsType.Integer:
                case ActivityDatumMetadata.DisplayUnitsType.BPM:
                case ActivityDatumMetadata.DisplayUnitsType.Percent:
                    return true;
                case ActivityDatumMetadata.DisplayUnitsType.None:
                default:
                    return false;
            }
        }

    }
}
