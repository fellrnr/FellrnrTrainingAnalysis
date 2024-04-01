namespace FellrnrTrainingAnalysis.Model
{

    public class AlignedTimeLocationSeries
    {
        public AlignedTimeLocationSeries(uint[] time, float[] lats, float[] lons, float[] secondary)
        {
            Time = time;
            Lats = lats;
            Lons = lons;
            Secondary = secondary;
        }

        public uint[] Time { get; set; }
        public float[] Lats { get; set; }
        public float[] Lons { get; set; }
        public float[] Secondary { get; set; }

        public static AlignedTimeLocationSeries? Align(LocationStream primary, TimeSeriesBase secondary, int forceCount, bool forceJustMe)
        {
            if (primary.Times == null)
                return null; //can't aling without time

            TimeValueList? sdata = secondary.GetData(forceCount, forceJustMe);
            if (sdata == null) return null;

            uint[] ptimes = primary.Times;
            float[] latvalues = primary.Latitudes;
            float[] lonvalues = primary.Longitudes;
            float[] svalues = sdata.Values;

            if (ptimes.Length == sdata.Length)
            {
                //let's assume they match
                AlignedTimeLocationSeries aligned = new AlignedTimeLocationSeries(ptimes, latvalues, lonvalues, svalues);
                return aligned;
            }
            else
            {
                List<uint> newTimes = new List<uint>();
                List<float> newLats = new List<float>();
                List<float> newLons = new List<float>();
                List<float> newSecondary = new List<float>();



                int si = 0;
                for (int pi = 0; pi < ptimes.Length && si < sdata.Length; pi++)
                {
                    while (pi < ptimes.Length && ptimes[pi] < si)
                    {
                        pi++;
                    }

                    while (si < sdata.Length && ptimes[pi] > si)
                    {
                        si++;
                    }

                    if (ptimes[pi] == si)
                    {
                        //all good
                        newTimes.Add(ptimes[pi]);
                        newLats.Add(latvalues[pi]);
                        newLons.Add(lonvalues[pi]);
                        newSecondary.Add(svalues[pi]);
                        if (si < sdata.Length)
                            si++;
                    }
                }


                AlignedTimeLocationSeries aligned = new AlignedTimeLocationSeries(newTimes.ToArray(),
                                                                                  newLats.ToArray(),
                                                                                  newLons.ToArray(),
                                                                                  newSecondary.ToArray());
                return aligned;
            }
        }

    }

}
