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
                    retval += " (datum null)"; 
                }
                else if(activityDatumMetadata == null)
                {
                    retval += " (meta null)";
                }
                else
                {
                    retval += $" [{datum}]";
                }
            }
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

            switch(activityDatumMetadata.DisplayUnits)
            {
                case ActivityDatumMetadata.DisplayUnitsType.Meters:
                    return FormatFloat(datum, "{0:#,0} m", 1.0f);
                case ActivityDatumMetadata.DisplayUnitsType.Kilometers:
                    return FormatFloat(datum, "{0:#,0.0} Km", 1.0f / 1000.0f);
                case ActivityDatumMetadata.DisplayUnitsType.Pace:
                    return FormatPace(datum);
                case ActivityDatumMetadata.DisplayUnitsType.TimeSpan:
                    return FormatTime(datum);
                case ActivityDatumMetadata.DisplayUnitsType.None:
                case ActivityDatumMetadata.DisplayUnitsType.Integer:
                case ActivityDatumMetadata.DisplayUnitsType.BPM:
                    return datum.ToString()!;
                default:
                    return "";
            }
        }

        private static string FormatFloat(Datum datum, string format, float mulitplier)
        {
            if(datum is not TypedDatum<float>) { return ""; }

            TypedDatum<float> floatDatum = (TypedDatum<float>)datum;
            float value = floatDatum.Data;
            value = value * mulitplier;
            return string.Format(format, value);
        }

        //pace is in 
        private static string FormatPace(Datum datum)
        {
            if (datum is not TypedDatum<float>) { return ""; }

            TypedDatum<float> floatDatum = (TypedDatum<float>)datum;
            float metersPerSecond = floatDatum.Data;
            float minutesPerKm = 16.666666667f / metersPerSecond; //https://www.aqua-calc.com/convert/speed/meter-per-second-to-minute-per-kilometer
            float secondsPerKm = minutesPerKm * 60;
            return FormatTime(secondsPerKm);

        }

        private static string FormatTime(Datum datum)
        {
            if (datum is not TypedDatum<float>) { return ""; }

            TypedDatum<float> floatDatum = (TypedDatum<float>)datum;
            float value = floatDatum.Data;
            return FormatTime(value);

        }

        private static string FormatTime(float totalSeconds)
        {
            float secInHour = 60 * 60;
            float hours = (float)Math.Floor(totalSeconds / secInHour);
            float remainder = totalSeconds % secInHour;
            float secInMin = 60;
            float mins = (float)Math.Floor(remainder / secInMin);
            float secs = remainder % secInMin;

            if (hours > 0)
            {
                return string.Format("{0}:{1:00}:{2:00}", hours, mins, secs);
            }
            else
            {
                return string.Format("{0}:{1:00}", mins, secs);
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
                    return true;
                case ActivityDatumMetadata.DisplayUnitsType.None:
                default:
                    return false;
            }
        }

    }
}
