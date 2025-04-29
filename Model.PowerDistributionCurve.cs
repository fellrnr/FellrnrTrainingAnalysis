using FellrnrTrainingAnalysis.Utils;
using GoogleApi.Entities.Maps.StreetView.Request.Enums;
using MemoryPack;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]
    public partial class PowerDistributionCurve : TimeSeriesEphemeral
    {
        [MemoryPackConstructor]
        protected PowerDistributionCurve()  //for use by memory pack deserialization only
        {
        }


        public PowerDistributionCurve(string name,
                                    Activity parent,
                                    bool persistCache,
                                    List<string>? requiredFields,
                                    List<string>? opposingFields = null,
                                    List<string>? sportsToInclude = null) :
            base(name, parent, persistCache, requiredFields, opposingFields, sportsToInclude)
        {
        }

        public override TimeValueList? CalculateData(int forceCount, bool forceJustMe)
        {
            if (forceJustMe) Logging.Instance.TraceEntry($"PowerDistributionCurve - Forced recalculating {this.Name}");

            TimeSeriesBase underlyingStream = RequiredTimeSeries[0];
            TimeValueList? underlyingData = underlyingStream.GetData(forceCount, forceJustMe);
            if (underlyingData == null || underlyingData.Length < 1)
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"No underlying stream");
                return null;
            }

            float[] Integrated = Integrate(underlyingData.Values);

            int duration = Options.Instance.MinimumTimeForPowerDistributionCurve;

            List<float> Values = new List<float>();
            while (duration < Integrated!.Length)
            {
                float totalEnergy = DividedMaxMean(Integrated, underlyingData.Length, duration);

                float watts = totalEnergy / duration; //undo the integration of watts * time

                Values.Add(watts);

                if (duration < 120) duration++;
                else if (duration < 600) duration += 2;
                else if (duration < 1200) duration += 5;
                else if (duration < 3600) duration += 20;
                else if (duration < 7200) duration += 120;
                else duration += 300;
            }



            TimeValueList result = new TimeValueList(Values.ToArray());

            int offset = PowerDistributionCurve.OneHourOffset();
            if (Values.Count > offset)
            {
                float OneHourPower = Values[offset];

                ParentActivity!.AddOrReplaceDatum(new TypedDatum<float>(Activity.Tag1HrPwr, false, OneHourPower));
            }

            if (forceJustMe) Logging.Instance.TraceLeave($"return PDC {result}");
            return result;
        }

        //hard code that one hour is 600 less the minimum time. TODO: make more flexible
        public static int OneHourOffset() { return 600 - Options.Instance.MinimumTimeForPowerDistributionCurve; }


        public static int[] GetTimes(TimeValueList tvl)
        {
            int duration = Options.Instance.MinimumTimeForPowerDistributionCurve;
            int[] result = new int[tvl.Length];
            for(int i = 0; i < tvl.Length; i++)
            {
                result[i] = duration;
                if (duration < 120) duration++;
                else if (duration < 600) duration += 2;
                else if (duration < 1200) duration += 5;
                else if (duration < 3600) duration += 20;
                else if (duration < 7200) duration += 120;
                else duration += 300;

            }
            return result;
        }


        float DividedMaxMean(float[] dataIntegrated, int sourceLength, int targetDuration)
        {
            int shift = targetDuration;

            //if sorting data the following is an important speedup hack
            if (shift > 180) shift = 180;

            int window_length = targetDuration + shift; //twice, or window length + 180 if smaller

            if (window_length > sourceLength) window_length = sourceLength;

            // put down as many windows as will fit without overrunning data
            int start;
            int end = 0;
            float energy;

            float candidate = 0;

            //scan in window increments
            for (start = 0; start + window_length <= sourceLength; start += shift)
            {
                end = start + window_length;
                energy = dataIntegrated[end] - dataIntegrated[start];

                if (energy < candidate)
                {
                    continue;
                }
                float window_mm = PartialMaxMean(dataIntegrated, start, end, targetDuration);

                if (window_mm > candidate)
                {
                    candidate = window_mm;
                }
            }

            // if the overlapping windows don't extend to the end of the data,
            // let's tack another one on at the end

            if (end < sourceLength)
            {
                start = sourceLength - window_length;
                end = sourceLength;
                energy = dataIntegrated[end] - dataIntegrated[start];

                if (energy >= candidate)
                {

                    float window_mm = PartialMaxMean(dataIntegrated, start, end, targetDuration);

                    if (window_mm > candidate)
                    {
                        candidate = window_mm;
                    }
                }
            }

            return candidate;
        }


        float PartialMaxMean(float[] dataIntegrated, int start, int end, int length)
        {
            int i;
            float candidate = 0;

            for (i = start; i < (1 + end - length); i++)
            {
                float test_energy = dataIntegrated[length + i] - dataIntegrated[i];
                if (test_energy > candidate)
                {
                    candidate = test_energy;
                }
            }
            return candidate;
        }

        float[] Integrate(float[] source)
        {
            //Note the integrated data is one longer than the source data
            float[] result = new float[source.Length + 1];
            float accumulator = 0;
            int i = 0;
            while (i < source.Length)
            {
                result[i] = accumulator;
                accumulator += source[i];
                i++;
            }
            result[i] = accumulator;

            return result;
        }

        public class BestCurve
        {
            public BestCurve(TimeValueList timeValueList, Dictionary<int, Activity> bestActivities, DateTime root)
            {
                TimeValueList = timeValueList;
                BestActivities = bestActivities;
                Root = root;
            }

            public DateTime Root;
            public TimeValueList TimeValueList { get; set; }

            //map of curve offset (not time!) to activity
            public Dictionary<int, Activity> BestActivities { get; set; }
        }

        public static BestCurve BestCurves(Dictionary<Activity, TimeValueList> curves, DateTime root)
        {
            int max = 0;
            foreach (var curve in curves)
            {
                max = Math.Max(max, curve.Value.Length);
            }

            float[] v = new float[max];
            Dictionary<int, Activity> bestActivities = new Dictionary<int, Activity>();
            foreach (var kvp in curves)
            {
                TimeValueList curve = kvp.Value;
                for (int i = 0; i < curve.Length; i++)
                {
                    if (i == PowerDistributionCurve.OneHourOffset() && curve.Values[i] > 300)
                        Logging.Instance.Debug("huh");
                    if (curve.Values[i] > v[i])
                    {
                        v[i] = curve.Values[i];
                        if (!bestActivities.ContainsKey(i))
                            bestActivities.Add(i, kvp.Key);
                        else
                            bestActivities[i] = kvp.Key;
                    }
                }
            }

            TimeValueList tvl = new TimeValueList(v);
            BestCurve best = new BestCurve(tvl, bestActivities, root);
            return best;
        }

    }
}


//----------------------------------------------------------------------
// Mark Rages' Algorithm for Fast Find of Mean-Max (From Golden Cheetah, RideFileCache.cpp)
//----------------------------------------------------------------------

/*

   A Faster Mean-Max Algorithm

   Premises:

   1 - maximum average power for a given interval occurs at maximum
       energy for the interval, because the interval time is fixed;

   2 - the energy in an interval enclosing a smaller interval will
       always be equal or greater than an interval;

   3 - finding maximum of means is a search algorithm, so biggest
       gains are found in reducing the search space as quickly as
       possible.

   Algorithm

   note: I find it easier to reason with concrete numbers, so I will
   describe the algorithm in terms of power and 60 second max-mean:

   To find the maximum average power for one minute:

   1 - integrate the watts over the entire ride to get accumulated
       energy in joules.  This is a monotonic function (assuming watts
       are positive).  The final value is the energy for the whole
       ride.  Once this is done, the energy for any section can be
       found with a single subtraction.

   2 - divide the energy into overlapping two-minute sections.
       Section one = 0:00 -> 2:00, section two = 1:00 -> 3:00, etc.

       Example:  Find 60s MM in 5-minute file

       +----------+----------+----------+----------+----------+
       | minute 1 | minute 2 | minute 3 | minute 4 | minute 5 |
       +----------+----------+----------+----------+----------+
       |             |_MEAN_MAX_|                             |
       +---------------------+---------------------+----------+
       |      segment 1      |      segment 3      |
       +----------+----------+----------+----------+----------+
                  |      segment 2      |      segment 4      |
                  +---------------------+---------------------+

       So no matter where the MEAN_MAX segment is located in time, it
       will be wholly contained in one segment.

       In practice, it is a little faster to make the windows smaller
       and overlap more:
       +----------+----------+----------+----------+----------+
       | minute 1 | minute 2 | minute 3 | minute 4 | minute 5 |
       +----------+----------+----------+----------+----------+
       |             |_MEAN_MAX_|                             |
       +-------------+----------------------------------------+
          |  segment 1  |
          +--+----------+--+
          |  segment 2  |
          +--+----------+--+
             |  segment 3  |
             +--+----------+--+
                |  segment 4  |
                +--+----------+--+
                   |  segment 5  |
                   +--+----------+--+
                      |  segment 6  |
                      +--+----------+--+
                         |  segment 7  |
                         +--+----------+--+
                            |  segment 8  |
                            +--+----------+--+
                               |  segment 9  |
                               +-------------+
                                            ... etc.

       ( This is because whenever the actual mean max energy is
         greater than a segment energy, we can skip the detail
         comparison within that segment altogether.  The exact
         tradeoff for optimum performance depends on the distribution
         of the data.  It's a pretty shallow curve.  Values in the 1
         minute to 1.5 minute range seem to work pretty well. )

   3 - for each two minute section, subtract the accumulated energy at
       the end of the section from the accumulated energy at the
       beginning of the section.  That gives the energy for that section.

   4 - in the first section, go second-by-second to find the maximum
       60-second energy.  This is our candidate for 60-second energy

   5 - go down the sorted list of sections.  If the energy in the next
       section is less than the 60-second energy in the best candidate so
       far, skip to the next section without examining it carefully,
       because the section cannot possibly have a one-minute section with
       greater energy.

       while (section->energy > candidate) {
         candidate=max(candidate, search(section, 60));
         section++;
       }

   6. candidate is the mean max for 60 seconds.

   Enhancements that are not implemented:

     - The two-minute overlapping sections can be reused for 59
       seconds, etc.  The algorithm will degrade to exhaustive search
       if the looked-for interval is much smaller than the enclosing
       interval.

     - The sections can be sorted by energy in reverse order before
       step #4.  Then the search in #5 can be terminated early, the
       first time it fails.  In practice, the comparisons in the
       search outnumber the saved comparisons.  But this might be a
       useful optimization if the windows are reused per the previous
       idea.

*/
