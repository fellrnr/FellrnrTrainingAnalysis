using FellrnrTrainingAnalysis.Model;
using FellrnrTrainingAnalysis.Utils;
using Microsoft.VisualBasic;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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

        public bool Success { get; set; } = true;
        public bool ActivityChanged { get; set; } = false;

        public void ProcessTags(Activity activity, int forceCount, bool forceJustMe, bool force)
        {
            TypedDatum<string>? descriptionDatum = (TypedDatum<string>?)activity.GetNamedDatum(Activity.TagDescription);
            if (descriptionDatum == null || descriptionDatum.Data == null)
                return;
            string description = descriptionDatum.Data;


            //change - only reprocess tags on forceJustMe, not every forced recalculate. 
            TypedDatum<string>? processedDatum = (TypedDatum<string>?)activity.GetNamedDatum(Activity.TagProcessedTags);
            string processedTags = (forceJustMe || processedDatum == null || processedDatum.Data == null) ? "" : processedDatum.Data;

            while (description.Contains(START) && description.Contains(END))
            {
                int start = description.IndexOf(START, StringComparison.Ordinal);
                int end = description.IndexOf(END, StringComparison.Ordinal);
                int len = end - start;
                string tag = description.Substring(start + 1, len - 1);

                if (!processedTags.Contains(tag))
                {
                    if (forceJustMe) Logging.Instance.Debug($"processedTags [{processedTags}] doesn't contain {tag}, so needs to be actioned");
                    ProcessTag(activity, tag, forceCount, forceJustMe);
                    if(!Success)
                    {
                        Logging.Instance.Error($"ProcessTag failed, activity {activity}");
                        return;
                    }
                    processedTags += tag;
                    ActivityChanged = true;
                }

                description = description.Substring(end + 1);
            }
            if (ActivityChanged)
            {
                if (forceJustMe) Logging.Instance.Debug($"processedTags is now {processedTags}");
                activity.AddOrReplaceDatum(new TypedDatum<string>(Activity.TagProcessedTags, true, processedTags)); //set recorded to true as this isn't something we want to recreate all the time
            }
        }

        private async void ProcessTag(Activity activity, string tag, int forceCount, bool forceJustMe)
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
            }
            else if (command == "Lookup")
            {
                if(target != Activity.TagAltitude)
                {
                    Logging.Instance.Error($"ProcessTag Lookup isn't altitude {target}");
                    Success = false;
                    return;
                }

                if (activity.LocationStream == null || activity.LocationStream.Times == null)
                {
                    Logging.Instance.Error($"ProcessTag Lookup without location data");
                    Success = false;
                    return;
                }

                Action.Elevation elevation = new Elevation();

                Task<TimeSeriesBase?> task = elevation.GetElevation(activity.LocationStream, activity);
                TimeSeriesBase? result = await task;
                if(result == null)
                {
                    Logging.Instance.Error($"ProcessTag Lookup failed");
                    Success = false;
                    return;
                }
                activity.AddTimeSeries(result);

            }
            else if (command == "CopyBack")
            {
                int amount = strings.Length > 2 ? int.Parse(strings[2]) : 0;
                if (forceJustMe) Logging.Instance.Debug($"ProcessTag command: copyback stream:{target}");
                if (!activity.TimeSeries.ContainsKey(target))
                {
                    Logging.Instance.Error($"ProcessTag CopyBack missing {target}");
                    Success = false;
                    return;
                }
                TimeSeriesBase dataStream = activity.TimeSeries[target];
                TimeValueList? data = dataStream.GetData(forceCount, forceJustMe);
                if (data == null || data.Length < amount)
                {
                    Logging.Instance.Error($"ProcessTag CopyBack {target} is too short");
                    Success = false;
                    return;
                }
                float copyback = data.Values[amount];

                for (int i = 0; i < amount; i++)
                {
                    data.Values[i] = copyback;
                }
                if (forceJustMe) Logging.Instance.Debug($"ProcessTag CopyBack {target} Done");
            }
            else if (command == "Cap")
            {
                int amount = strings.Length > 2 ? int.Parse(strings[2]) : 0;
                if (forceJustMe) Logging.Instance.Debug($"ProcessTag Cap {target} to {amount}");
                TimeSeriesBase dataStream = activity.TimeSeries[target];
                TimeValueList? data = dataStream.GetData(forceCount, forceJustMe);
                if (data == null)
                {
                    Logging.Instance.Error($"ProcessTag Cap {target} no data");
                    Success = false;
                }
                else
                {
                    for (int i = 0; i < data.Values.Length; i++)
                    {
                        if (data.Values[i] > amount)
                            data.Values[i] = amount;
                    }
                    if (forceJustMe) Logging.Instance.Debug($"ProcessTag Cap {target} Done");
                }
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
            }
            else
            {
                Logging.Instance.Error($"ProcessTag unexpected command {command}");
                Success = false;
                return;
            }
        }


        public void TagStravaActivity(string tag, Activity activity, bool process = true)
        {
            TypedDatum<string>? descriptionDatum = (TypedDatum<string>?)activity.GetNamedDatum(Activity.TagDescription);
            if (descriptionDatum == null)
                descriptionDatum = new TypedDatum<string>(Activity.TagDescription, true, ""); //make this recoreded as we need it to persist

            string? description = descriptionDatum.Data;

            if (description != null && !description.Contains(tag))
            {
                description = description + tag;

                //HACK: short term fix to migrate from deleting altitude to looking it up
                if (description.Contains($"⌗{Activity.TagAltitude}༶Lookup֍") && description.Contains($"⌗{Activity.TagAltitude}༶Delete֍"))
                {
                    description = description.Replace($"⌗{Activity.TagAltitude}༶Delete֍", "");
                }

                if (!Action.StravaApi.Instance.UpdateActivityDetails(activity, null, description))
                {
                    MessageBox.Show("Update Failed");
                    return;
                }
                descriptionDatum.Data = description;
                activity.AddOrReplaceDatum(descriptionDatum);

                if(process)
                    this.ProcessTags(activity, 0, true, true); //force and ask for debug
            }
        }

    }
}
