using FellrnrTrainingAnalysis.Model;
using FellrnrTrainingAnalysis.Utils;

namespace FellrnrTrainingAnalysis.Action
{
    public class Tags
    {

        public const string START = "⌗";
        public const string END = "֍";
        public const char MIDDLE = '༶';

        //start char is ⌗ U+2317
        //middle markers are ༶ (U+0F36)
        //end is ֍ (U+058D)
        //Form "⌗<field/stream>༶<command>༶<parm1>༶<parm2>༶<paramN>֍
        //
        //new TagActivities("Delete Altitude", "⌗Altitude༶Delete֍"),
        //new TagActivities("Replace Start of Altitude", "⌗Altitude༶CopyBack༶10֍"),
        //new TagActivities("Delete Power", "⌗Power༶Delete֍"),
        //new TagActivities("Cap Power CP", "⌗Power༶Cap༶100֍"),

        public bool ProcessTags(Activity activity, int forceCount, bool forceJustMe, bool force)
        {
            TypedDatum<string>? descriptionDatum = (TypedDatum<string>?)activity.GetNamedDatum(Activity.TagDescription);
            if (descriptionDatum == null || descriptionDatum.Data == null)
                return false;
            string description = descriptionDatum.Data;

            //change - only reprocess tags on forceJustMe, not every forced recalculate. 
            TypedDatum<string>? processedDatum = (TypedDatum<string>?)activity.GetNamedDatum(Activity.TagProcessedTags);
            string processedTags = (forceJustMe || processedDatum == null || processedDatum.Data == null) ? "" : processedDatum.Data;
            bool processedTagsChanged = false;

            while (description.Contains(START) && description.Contains(END))
            {
                int start = description.IndexOf(START, StringComparison.Ordinal);
                int end = description.IndexOf(END, StringComparison.Ordinal);
                int len = end - start;
                string tag = description.Substring(start + 1, len - 1);

                if (!processedTags.Contains(tag))
                {
                    if (forceJustMe) Logging.Instance.Debug($"processedTags [{processedTags}] doesn't contain {tag}, so needs to be actioned");
                    if (!ProcessTag(activity, tag, forceCount, forceJustMe))
                    {
                        Logging.Instance.Error($"ProcessTag failed, activity {activity}");
                        return false;
                    }
                    processedTags += tag;
                    processedTagsChanged = true;
                }

                description = description.Substring(end + 1);
            }
            if (processedTagsChanged)
            {
                if (forceJustMe) Logging.Instance.Debug($"processedTags is now {processedTags}");
                activity.AddOrReplaceDatum(new TypedDatum<string>(Activity.TagProcessedTags, true, processedTags)); //set recorded to true as this isn't something we want to recreate all the time

                if (!force) //we weren't forced, then we need to force downstream processing)
                    return true;
            }
            return false;
        }

        private bool ProcessTag(Activity activity, string tag, int forceCount, bool forceJustMe)
        {
            if (forceJustMe) Logging.Instance.Debug($"ProcessTag({tag})");
            string[] strings = tag.Split(MIDDLE);
            string target = strings[0];
            string command = strings[1];

            if (command == "Delete")
            {
                if (forceJustMe) Logging.Instance.Debug($"ProcessTag command: delete stream:{target}");
                activity.RemoveNamedDatum(target);
                activity.RemoveTimeSeries(target);
                return true;
            }
            else if (command == "CopyBack")
            {
                int amount = strings.Length > 2 ? int.Parse(strings[2]) : 0;
                if (forceJustMe) Logging.Instance.Debug($"ProcessTag command: copyback stream:{target}");
                if (!activity.TimeSeries.ContainsKey(target))
                {
                    if (forceJustMe) Logging.Instance.Debug($"ProcessTag CopyBack missing {target}");
                    return false;
                }
                TimeSeriesBase dataStream = activity.TimeSeries[target];
                TimeValueList? data = dataStream.GetData(forceCount, forceJustMe);
                if (data == null || data.Length < amount)
                {
                    if (forceJustMe) Logging.Instance.Debug($"ProcessTag CopyBack {target} is too short");
                    return false;
                }
                float copyback = data.Values[amount];

                for (int i = 0; i < amount; i++)
                {
                    data.Values[i] = copyback;
                }
                if (forceJustMe) Logging.Instance.Debug($"ProcessTag CopyBack {target} Done");
                return true;
            }
            else if (command == "Cap")
            {
                int amount = strings.Length > 2 ? int.Parse(strings[2]) : 0;
                if (forceJustMe) Logging.Instance.Debug($"ProcessTag Cap {target} to {amount}");
                TimeSeriesBase dataStream = activity.TimeSeries[target];
                TimeValueList? data = dataStream.GetData(forceCount, forceJustMe);
                if (data == null)
                {
                    if (forceJustMe) Logging.Instance.Debug($"ProcessTag Cap {target} no data");
                    return false;
                }
                for (int i = 0; i < data.Values.Length; i++)
                {
                    if (data.Values[i] > amount)
                        data.Values[i] = amount;
                }
                if (forceJustMe) Logging.Instance.Debug($"ProcessTag Cap {target} Done");
                return true;
            }
            else if (command == "Override")
            {
                //⌗Distance༶Override༶9656֍
                float amount = strings.Length > 2 ? float.Parse(strings[2]) : 0;

                activity.AddOrReplaceDatum(new TypedDatum<float>(target, true, amount)); //set recorded to true, otherwise we'll delete them on recalculate and ignore the override becaseu we've saved the processed tags
                if (forceJustMe) Logging.Instance.Debug($"ProcessTag Override {target} datum to {amount}");

                if (activity.TimeSeries.ContainsKey(target))
                {
                    if (forceJustMe) Logging.Instance.Debug($"ProcessTag Deleting TimeSeries with same name as Override {target}");
                    activity.RemoveTimeSeries(target);
                }

                return true;
            }
            else
            {
                Logging.Instance.Error($"ProcessTag unexpected command {command}");
                return false;
            }
        }
    }
}
