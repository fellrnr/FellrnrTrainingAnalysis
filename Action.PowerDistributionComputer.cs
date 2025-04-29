using FellrnrTrainingAnalysis.Model;
using FellrnrTrainingAnalysis.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FellrnrTrainingAnalysis.Action
{
    /*
    public class PowerDistributionComputer
    {
        public PowerDistributionComputer(float[] source)
        {
            Source = source;
            Curve = null;

            if (Source == null) return;
            if (Source.Length < Options.Instance.MinimumTimeForPowerDistributionCurve) return;


            Integrated = Integrate(Source);

            ComputeCurve();
        }

        float[]? Source { get; set; }
        float[]? Integrated { get; set; }

        List<int> Times = new List<int>();
        List<float> Values = new List<float>();

        public PowerDistributionCurve? Curve { get; set; }

        private void ComputeCurve()
        {
            int duration = Options.Instance.MinimumTimeForPowerDistributionCurve;
            while (duration < Integrated!.Length)
            {
                float totalEnergy = DividedMaxMean(Integrated, Source!.Length, duration);

                float watts = totalEnergy / duration; //undo the integration of watts * time

                Times.Add(duration);
                Values.Add(watts);

                if (duration < 120) duration++;
                else if (duration < 600) duration += 2;
                else if (duration < 1200) duration += 5;
                else if (duration < 3600) duration += 20;
                else if (duration < 7200) duration += 120;
                else duration += 300;
            }

            //Curve = new PowerDistributionCurve(Times.ToArray(), Values.ToArray());
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
            float[] result = new float[source.Length+1];
            float accumulator = 0;
            int i = 0;
            while(i < source.Length)
            {
                result[i] = accumulator;
                accumulator += source[i];
                i++;
            }
            result[i] = accumulator;

            return result;
        }
    }
    */
}
