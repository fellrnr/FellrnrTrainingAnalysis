using MemoryPack;
using FellrnrTrainingAnalysis.Model;
using System.Collections.ObjectModel;
using System.Text;

namespace FellrnrTrainingAnalysis.Utils
{

    public class DataQuality
    {
        List<DataQualityCheck> dataQualityCheckList = new List<DataQualityCheck>();
        public DataQuality()
        {

            //No change is benign, and can be real if a very flat course
            //dataQualityCheckList.Add(new DataQualityCheckStuckValue("Altitude has no change for too far", "Altitude", "Distance", new DataRemediationNoAction(), 1000));

            dataQualityCheckList.Add(new DataQualityHeartRate("Heart Rate Wrong", new DataRemediationNoAction(), minHrPwr: 20, maxHr: 190));

            dataQualityCheckList.Add(new DataQualityCheckFixedValue("Altitude fixed value (indoor?)", "Altitude", null, new DataRemediationNoAction()));

            //seems to be a common problem, and easily resolved
            dataQualityCheckList.Add(new DataQualityCheckRapidAbsChange("Altitude initial jumps", "Altitude", null, new DataRemediationBackCopyStream("Altitude", 10), maxAbsChangePerSecondAllowed: 10, onlyScanFirstN: 10));

            //10 m/s vertical change is too much to be real
            dataQualityCheckList.Add(new DataQualityCheckRapidAbsChange("Altitude jumps", "Altitude", null, new DataRemediationRemoveStream("Altitude"), maxAbsChangePerSecondAllowed: 10));

            //a bit of error at the coast can result in below sea level readings, and only worry if altitude goes high as well
            dataQualityCheckList.Add(new DataQualityCheckLimits("Altitude below sea level", "Altitude", new DataRemediationRemoveStream("Altitude"), min: -50f, max: 1000f, andLimits: true));

            dataQualityCheckList.Add(new DataQualityCheckLimits("Altitude too high", "Altitude", new DataRemediationRemoveStream("Altitude"), max: 4600f, andLimits: false));

            dataQualityCheckList.Add(new DataQualityCheckZeros("No Distance data stream", "Distance", new DataRemediationRemoveStream("Distance")));

            dataQualityCheckList.Add(new DataQualityCheckLimits("Heart Rate too high", "Heart Rate", new DataRemediationRemoveStream("Heart Rate"), max: 200f, andLimits: false));

            dataQualityCheckList.Add(new DataQualityCheckLimits("Heart Rate test", "Heart Rate", new DataRemediationRemoveStream("Heart Rate"), max: 100f, andLimits: false));

            //TODO: Problem - recovery is confused it the timer is paused
            //60 beats in 60 seconds in 95th percentile of athletes
            dataQualityCheckList.Add(new DataQualityCheckRapidChange("Heart Rate recovery too fast (60 in 60)", "Heart Rate", null, new DataRemediationRemoveStream("Heart Rate"), maxDecreaseAllowed: -60, period: 60));
            dataQualityCheckList.Add(new DataQualityCheckRapidChange("Heart Rate recovery too fast (60 in 90)", "Heart Rate", null, new DataRemediationRemoveStream("Heart Rate"), maxDecreaseAllowed: -60, period: 90));
        }

        public ReadOnlyCollection<DataQualityCheck> CheckList { get {  return dataQualityCheckList.AsReadOnly(); } }

        public List<String> FindBadTimeSeries(Database database, DataQualityCheck? check = null, bool fix=false)
        {
            List<String> badStreams = new List<String>();

            if (database == null || database.CurrentAthlete == null) { return badStreams; }

            foreach (KeyValuePair<DateTime, Activity> kvp in database.CurrentAthlete.ActivitiesByUTCDateTime)
            {
                Activity activity = kvp.Value;

                List<string> badActivitystreams = FindBadTimeSeries(activity, check, fix);


                badStreams.AddRange(badActivitystreams);

            }

            return badStreams;
        }

        public List<string> FindBadTimeSeries(Activity activity, DataQualityCheck? check = null, bool fix = false)
        {
            List<string> badStreams = new List<string>();
            activity.ClearDataQualityIssues();


            if (check != null)
            {
                badStreams.AddRange(check.FindBadData(activity, fix));
            }
            else
            {
                foreach (DataQualityCheck dqr in dataQualityCheckList)
                {
                    badStreams.AddRange(dqr.FindBadData(activity, fix));
                }
            }
            return badStreams;
        }
    }


    public abstract class DataQualityCheck
    {

        public DataQualityCheck(string description, DataRemediation dataRemediation)
        {
            DataRemediation = dataRemediation;
            Description = description;
        }
        DataRemediation DataRemediation;
        public string Description { get; }

        public List<string> FindBadData(Activity activity, bool fix)
        {
            List<string> retval = FindBadData(activity);

            if(fix && retval.Count > 0)
            {
                DataRemediation.Clean(activity);
            }
            return retval;
        }

        protected abstract List<string> FindBadData(Activity activity);

        public abstract string Reason(Activity activity, TimeSeriesBase dataStream, string reason);
    }


    public class DataQualityHeartRate : DataQualityCheck
    {
        public DataQualityHeartRate(string description, DataRemediation dataRemediation, float minHrPwr, float maxHr) : base(description, dataRemediation)
        {
            MaxHr = maxHr;
            MinHrPwr = minHrPwr;
        }
        float MinHrPwr;
        float MaxHr;

        protected override List<string> FindBadData(Activity activity)
        {
            List<string> badStreams = new List<string>();

            if(!activity.TimeSeries.ContainsKey(Activity.TagHeartRate))
                return badStreams;

            if (!activity.TimeSeries.ContainsKey(Activity.TagHrPwr))
                return badStreams;

            TimeSeriesBase hr = activity.TimeSeries[Activity.TagHeartRate];
            TimeSeriesBase hrpwr = activity.TimeSeries[Activity.TagHrPwr];
            TimeValueList? hrd = hr.GetData(forceCount: 0, forceJustMe: true);
            TimeValueList? hrpwrd = hrpwr.GetData(forceCount: 0, forceJustMe: true);
            if (hrd == null || hrpwrd == null)
                return badStreams;

            List<Tuple<uint, uint>> TooHigh = new List<Tuple<uint, uint>>();

            uint startOfBad = 0;
            int suspicion = 0;
            bool inbad = false;
            uint lastTime = 0;
            for(int i=0; i < hrd.Length; i++)
            {
                if (hrd.Values[i] > MaxHr)
                {
                    if (!inbad)
                        startOfBad = (uint)i;
                    inbad = true;
                    suspicion = 30;
                }
                else
                {
                    if (inbad)
                    {
                        int deltat = (int)hrd.Values[i] - (int)lastTime;
                        suspicion -= deltat;
                        if (suspicion < 0 || i == hrd.Length-1) //grab problems that go to the end
                        {
                            inbad = false;
                            Tuple<uint, uint> area = new Tuple<uint, uint>(startOfBad, (uint)i);
                            TooHigh.Add(area);
                            hr.AddHighlight(area);
                            badStreams.Add($"Over {MaxHr} from {startOfBad} to {i}, {activity}");
                        }
                    }
                }
                lastTime = hrd.Times[i];
            }

            if(badStreams.Count > 0)
            {
                if (activity.DataQualityIssues == null)
                    activity.DataQualityIssues = badStreams;
                else
                    activity.DataQualityIssues.AddRange(badStreams);
            }

            return badStreams;
        }

        public override string Reason(Activity activity, TimeSeriesBase dataStream, string reason)
        {
            return string.Format("Activity {0}, stream {1}, reason {2}", activity.ToString(), dataStream.ToString(), reason);
        }
    }


    public abstract class DataQualityCheckStream : DataQualityCheck
    {

        public DataQualityCheckStream(string description, string targetTimeSeries, string? xaxis, DataRemediation dataRemediation) : base(description, dataRemediation)
        {
            TargetTimeSeries = targetTimeSeries;
            xAxisName = xaxis;
        }
        private string TargetTimeSeries;
        private string? xAxisName;

        protected float XValueAtTimeT(TimeSeriesBase? dataStreamX, int offsetEstimate, uint timeT, Activity activity)
        {
            if (dataStreamX == null)
            {
                return timeT;
            }

            TimeValueList? data = dataStreamX.GetData(forceCount: 0, forceJustMe: false);
            if (data == null || data.Times.Length < 2)
                return timeT;

            uint[] times = data.Times;
            float[] values = data.Values;

            if (times[offsetEstimate] == timeT) //most likely, the time sequence for both data streams will match. If it does, we know which value to use
                return values[offsetEstimate];


            //if not, we have to search for the right time, but this can take an exponential time to complete
            for (int i = 0; i < times.Length; i++)
            {
                if (timeT <= times[i])
                {
                    return values[i];
                }
            }

            return timeT;
        }

        protected override List<string> FindBadData(Activity activity)
        {
            List<string> badStreams = new List<string>();

            if (!activity.TimeSeries.ContainsKey(TargetTimeSeries))
            {
                return badStreams;
            }
            TimeSeriesBase dataStream = activity.TimeSeries[TargetTimeSeries];

            if (xAxisName != null && !activity.TimeSeries.ContainsKey(xAxisName))
            {
                return badStreams;
            }
            TimeSeriesBase? dataStreamX = xAxisName == null ? null : activity.TimeSeries[xAxisName];

            string? error = CheckTimeSeries(dataStream, dataStreamX, activity);
            if (error != null)
            {
                string badStream = Reason(activity, dataStream, error);
                badStreams.Add(badStream);

                if (activity.DataQualityIssues == null)
                    activity.DataQualityIssues = new List<string> { error };
                else
                    activity.DataQualityIssues.Add(error);
            }

            return badStreams;
        }

        protected abstract string? CheckTimeSeries(TimeSeriesBase dataStream, TimeSeriesBase? dataStreamX, Activity activity);

        //TODO the Reason method is a bit redundant, or at least, ugly
        public override string Reason(Activity activity, TimeSeriesBase dataStream, string reason)
        {
            return string.Format("Activity {0}, stream {1}, reason {2}", activity.ToString(), dataStream.ToString(), reason);
        }
    }





    public class DataQualityCheckStuckValue : DataQualityCheckStream
    {
        public DataQualityCheckStuckValue(string description, string target, string? xaxis, DataRemediation dataRemediation, float? maxAllowedXSpanWithNoYChange) : base(description, target, xaxis, dataRemediation)
        {
            MaxAllowedXSpanWithNoYChange = maxAllowedXSpanWithNoYChange;
        }
        private float? MaxAllowedXSpanWithNoYChange;

        protected override string? CheckTimeSeries(TimeSeriesBase dataStream, TimeSeriesBase? dataStreamX, Activity activity)
        {
            TimeValueList? data = dataStream.GetData(forceCount: 0, forceJustMe: false);
            if (data == null || data.Times.Length < 2)
                return null;

            //the X axis is either time or distance

            uint[] times = data.Times;
            float[] yValues = data.Values;
            float lastYValue = yValues[0];
            float lastXValue = XValueAtTimeT(dataStreamX, 0, times[0], activity);

            if (yValues.Max() == yValues.Min()) //fixed values are not our problem
                return null;

            float xValueAtLastYChange = lastXValue;

            float maxFixedXSpan = 0;
            float yValueWhileFixed = -1;
            float timeAtEndOfFixedPeriod = -1;
            float xAtEndOfFixedPeriod = -1;

            for (int i = 1; i < yValues.Length; i++) //start from 1 as we're looking at the next value
            {
                float currentYValue = yValues[i];
                float currentXValue = XValueAtTimeT(dataStreamX, i, times[i], activity);

                if (MaxAllowedXSpanWithNoYChange != null)
                {
                    if (lastYValue != currentYValue)
                    {
                        xValueAtLastYChange = currentXValue;
                    }
                    else if ((currentXValue - xValueAtLastYChange) > MaxAllowedXSpanWithNoYChange)
                    {
                        if ((currentXValue - xValueAtLastYChange) > maxFixedXSpan)
                        {
                            maxFixedXSpan = currentXValue - xValueAtLastYChange;
                            yValueWhileFixed = currentYValue;
                            timeAtEndOfFixedPeriod = times[i];
                            xAtEndOfFixedPeriod = currentXValue;
                        }
                    }
                }
                lastYValue = currentYValue;
            }

            if (MaxAllowedXSpanWithNoYChange != null && maxFixedXSpan > MaxAllowedXSpanWithNoYChange)
            {
                float min = yValues.Min();
                float max = yValues.Max();
                if (dataStreamX != null)
                    return $"maxTimeWithNoChange exceeded in {dataStream.Name} against {dataStreamX.Name}. Value fixed at {yValueWhileFixed} for {maxFixedXSpan} {dataStreamX.Name}, happened at {timeAtEndOfFixedPeriod} seconds, {xAtEndOfFixedPeriod} {dataStreamX.Name}. (min {min}, max {max})";
                else
                    return $"maxTimeWithNoChange exceeded in {dataStream.Name}. Value fixed at {yValueWhileFixed} for {maxFixedXSpan} seconds, happened at {timeAtEndOfFixedPeriod} seconds. (min {min}, max {max})";
            }

            return null;
        }
    }

    public class DataQualityCheckRapidAbsChange : DataQualityCheckStream
    {
        public DataQualityCheckRapidAbsChange(string description, string target, string? xaxis, DataRemediation dataRemediation, float maxAbsChangePerSecondAllowed, int? onlyScanFirstN = null) : base(description, target, xaxis, dataRemediation)
        {
            MaxAbsChangePerSecondAllowed = maxAbsChangePerSecondAllowed;
            OnlyScanFirstN = onlyScanFirstN;
        }
        private float MaxAbsChangePerSecondAllowed;
        private int? OnlyScanFirstN;

        protected override string? CheckTimeSeries(TimeSeriesBase dataStream, TimeSeriesBase? dataStreamX, Activity activity)
        {
            TimeValueList? data = dataStream.GetData(forceCount: 0, forceJustMe: false);
            if (data == null || data.Times.Length < 2)
                return null;

            //the X axis is either time or distance



            uint[] times = data.Times;
            float[] yValues = data.Values;

            float lastYValue = yValues[0];
            float lastXValue = XValueAtTimeT(dataStreamX, 0, times[0], activity);

            if (yValues.Max() == yValues.Min()) //fixed values are not our problem
                return null;

            for (int i = 1; i < yValues.Length && (OnlyScanFirstN == null || i < OnlyScanFirstN); i++) //start from 1 as we're looking at the next value
            {
                float currentYValue = yValues[i];
                float currentXValue = XValueAtTimeT(dataStreamX, i, times[i], activity);

                float valueDelta = Math.Abs(currentYValue - lastYValue);
                float xValueDelta = currentXValue - lastXValue;
                float rateOfChange = valueDelta / xValueDelta;
                if (xValueDelta > 0 && rateOfChange > MaxAbsChangePerSecondAllowed)
                {
                    return string.Format("maxAbsChangePerSample exceeded in {0}, change {1}, time {2}", dataStream.Name, valueDelta, xValueDelta);
                }
                lastYValue = currentYValue;
            }
            return null;
        }
    }

    public class DataQualityCheckRapidChange : DataQualityCheckStream
    {
        public DataQualityCheckRapidChange(string description, string target, string? xaxis, DataRemediation dataRemediation, 
            float? maxIncreaseAllowed = null, float? maxDecreaseAllowed = null, float? period = null) : base(description, target, xaxis, dataRemediation)
        {
            MaxIncreaseAllowed = maxIncreaseAllowed;
            MaxDecreaseAllowed = maxDecreaseAllowed;
            Period = period;
        }
        private float? MaxIncreaseAllowed;
        private float? MaxDecreaseAllowed;
        private float? Period;

        protected override string? CheckTimeSeries(TimeSeriesBase dataStream, TimeSeriesBase? dataStreamX, Activity activity)
        {
            TimeValueList? data = dataStream.GetData(forceCount: 0, forceJustMe: false);
            if (data == null || data.Times.Length < 2)
                return null;

            //the X axis is either time or distance



            uint[] times = data.Times;
            float[] yValues = data.Values;
            TimeValueList? convertedToDelta;

            if (Period == null || Period == 1)
            {
                convertedToDelta = TimeValueList.SimpleDeltas(data, 1, null, null);
            }
            else
            {
                convertedToDelta = TimeValueList.SpanDeltas(data, 1, null, null, (float)Period, false);
            }
            if (convertedToDelta == null)
                return null;

            float[] convertedYValues = convertedToDelta.Values;
            float max = convertedYValues.Max();
            float min = convertedYValues.Min();
            if (MaxIncreaseAllowed != null && max > MaxIncreaseAllowed)
            {
                return $"Rapid changed exceeded in {dataStream.Name}, max increase {max}, limit {MaxIncreaseAllowed}, in {Period}";
            }
            if (MaxDecreaseAllowed != null && min < MaxDecreaseAllowed)
            {
                return $"Rapid changed exceeded in {dataStream.Name}, max decrease {min}, limit {MaxDecreaseAllowed}, in {Period}";
            }
            return null;
        }
    }

    public class DataQualityCheckZeros : DataQualityCheckStream
    {
        public DataQualityCheckZeros(string description, string target, DataRemediation dataRemediation) : base(description, target, null, dataRemediation)
        {
        }

        protected override string? CheckTimeSeries(TimeSeriesBase dataStream, TimeSeriesBase? dataStreamX, Activity activity)
        {
            TimeValueList? data = dataStream.GetData(forceCount: 0, forceJustMe: false);
            if (data == null || data.Times.Length < 2)
                return null;

            float[] yValues = data.Values;
            if (yValues.Max() == 0 && yValues.Min() == 0)
            {
                return string.Format("All data is zero in {0}", dataStream.Name);
            }
            return null;
        }
    }



    public class DataQualityCheckLimits : DataQualityCheckStream
    {
        public DataQualityCheckLimits(string description, string target, DataRemediation dataRemediation, float? min = null, float? max = null, bool andLimits = true) : base(description, target, null, dataRemediation)
        {
            Max = max;
            Min = min;
            AndLimits = andLimits;
            if (AndLimits && (Max == null || Min == null))
                throw new ArgumentException("For DataQualityCheckLimits to AND the limits, both min and max must be provided");
        }
        float? Min;
        float? Max;
        bool AndLimits; //and them together

        protected override string? CheckTimeSeries(TimeSeriesBase dataStream, TimeSeriesBase? dataStreamX, Activity activity)
        {
            TimeValueList? data = dataStream.GetData(forceCount: 0, forceJustMe: false);
            if (data == null || data.Times.Length < 2)
                return null;

            float[] yValues = data.Values;

            if (Min != null && yValues.Min() < Min && !AndLimits)
            {
                return $"Range exceeded in {dataStream.Name}, min limit {Min}, actual min {yValues.Min()}";
            }


            if (Max != null && yValues.Max() > Max && !AndLimits)
            {
                return $"Range exceeded in {dataStream.Name}, max limit {Max}, actual max {yValues.Max()}";
            }

            if (Min != null && yValues.Min() < Min && Max != null && yValues.Max() > Max && AndLimits)
            {
                return $"Range exceeded in {dataStream.Name}, both max {Max}, actual max {yValues.Max()}, min limit {Min}, actual min {yValues.Min()}";
            }

            return null;
        }
    }



    public class DataQualityCheckFixedValue : DataQualityCheckStream
    {
        public DataQualityCheckFixedValue(string description, string target, string? xaxis, DataRemediation dataRemediation) : base(description, target, xaxis, dataRemediation)
        {
        }

        protected override string? CheckTimeSeries(TimeSeriesBase dataStream, TimeSeriesBase? dataStreamX, Activity activity)
        {
            TimeValueList? data = dataStream.GetData(forceCount: 0, forceJustMe: false);
            if (data == null || data.Times.Length < 2)
                return null;

            float[] values = data.Values;
            if (values.Max() == values.Min())
            {
                return string.Format($"Data stream {dataStream.Name} has a fixed, unchanging value ${values.Max()}");
            }

            return null;
        }

    }

    public class DataQualityCheckMissingDataField : DataQualityCheck
    {
        public DataQualityCheckMissingDataField(string description, string target, DataRemediation dataRemediation) : base(description, dataRemediation)
        {
            Target = target;
        }
        private string Target;


        protected override List<string> FindBadData(Activity activity)
        {
            List<string> badStreams = new List<string>();

            if (!activity.DataNames.Contains(Target))
            {
                badStreams.Add($"No data field {Target} found");
            }

            return badStreams;
        }

        public override string Reason(Activity activity, TimeSeriesBase dataStream, string reason)
        {
            return string.Format("Activity {0}, reason {2}", activity.ToString(), dataStream.ToString(), reason);
        }

    }

    public class DataQualityCheckMissingTimeSeries : DataQualityCheck
    {
        public DataQualityCheckMissingTimeSeries(string description, string target, DataRemediation dataRemediation) : base(description, dataRemediation)
        {
            Target = target;
        }
        private string Target;


        protected override List<string> FindBadData(Activity activity)
        {
            List<string> badStreams = new List<string>();

            if (!activity.HasNamedDatum(Target))
            {
                badStreams.Add($"No data stream {Target} found");
            }

            return badStreams;
        }

        public override string Reason(Activity activity, TimeSeriesBase dataStream, string reason)
        {
            return string.Format("Activity {0}, reason {2}", activity.ToString(), dataStream.ToString(), reason);
        }

    }


    public abstract class DataRemediation
    {
        protected DataRemediation() { }

        public abstract void Clean(Activity activity);
    }

    public class DataRemediationRemoveStream : DataRemediation
    {
        public DataRemediationRemoveStream(string target) { Target = target; }
        string Target;

        public override void Clean(Activity activity)
        {
            activity.RemoveNamedDatum(Target);
            activity.RemoveTimeSeries(Target);
            activity.Recalculate(true); //recalculate anything that might depend on this data
        }
    }

    public class DataRemediationBackCopyStream : DataRemediation
    {
        public DataRemediationBackCopyStream(string target, int position) { Target = target; Position = position;  }
        string Target;
        int Position;

        public override void Clean(Activity activity)
        {
            if (!activity.TimeSeries.ContainsKey(Target))
                return;
            TimeSeriesBase dataStream = activity.TimeSeries[Target];
            TimeValueList? data = dataStream.GetData(forceCount: 0, forceJustMe: false);
            if (data == null || data.Times.Length < Position)
                return;

            float copyback = data.Values[Position];

            for (int i = 0; i < Position; i++)
            {
                data.Values[i] = copyback;
            }
        }
    }



    public class DataRemediationNoAction : DataRemediation
    {
        public DataRemediationNoAction() { }

        public override void Clean(Activity activity)
        {
        }
    }
}
