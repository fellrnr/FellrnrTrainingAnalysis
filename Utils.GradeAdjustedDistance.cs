﻿using FellrnrTrainingAnalysis.Model;
using static FellrnrTrainingAnalysis.Utils.TimeSeries;

namespace FellrnrTrainingAnalysis.Utils
{
    public class GradeAdjustedDistance
    {

        public GradeAdjustedDistance(AlignedTimeSeries aligned)
        {
            Logging.Instance.ContinueAccumulator($"Utils.GradeAdjustedDistance.GradeAdjustedDistance");
            Aligned = aligned;
            if (Aligned.Time.Length < 2)
                throw new ArgumentException("need at least two distances to calculate GAP");

            //calculate the initial deltas for distance and altitude

            _distanceDeltas = CalculateDelta(Aligned.Primary);

            _altitudeDeltas = CalculateDelta(Aligned.Secondary);

            //calcualte grade from distance and altitude deltas
            _rawGrades = CalculateGrade(_distanceDeltas, _altitudeDeltas);

            //smoothing the grade is effectively the same a weighted grading the altitude, but simpler
            //we have to smooth the grade, as the cost is a polynomial of the grade, so it's too late to smooth then; any errors will be magnified 
            if (Options.Instance.GADSmoothingType == TimeSeries.SmoothingOptions.AverageWindow)
                _smoothedGrades = TimeSeries.WindowSmoothed(_rawGrades, Options.Instance.GADSmoothingWindow);
            else if (Options.Instance.GADSmoothingType == TimeSeries.SmoothingOptions.SimpleExponential)
                _smoothedGrades = TimeSeries.SimpleExponentialSmoothed(_rawGrades, Options.Instance.GADSmoothingWindow);
            else
                _smoothedGrades = _rawGrades;

            _costs = CalculateCost(_smoothedGrades);

            _gradeAdjustedDistance = CalculateGradeAdjustedDistance(_distanceDeltas, _costs);

            Logging.Instance.PauseAccumulator($"Utils.GradeAdjustedDistance.GradeAdjustedDistance");
        }


        /*
        public GradeAdjustedDistance(TimeDataTuple distanceOrSpeed, TimeDataTuple altitudes) 
        { 
            this.DistanceOrSpeed = distanceOrSpeed;
            this.Altitudes = altitudes;
            if (DistanceOrSpeed.Item1.Length < 2)
                throw new ArgumentException("need at least two distances to calculate GAP");
            if (DistanceOrSpeed.Item1.Length != altitudes.Item1.Length)
                throw new ArgumentException("distance and altitude must have the same number of entries");

            //calculate the initial deltas for distance and altitude

            _distanceDeltas = CalculateDelta(DistanceOrSpeed.Item2);

            _altitudeDeltas = CalculateDelta(Altitudes.Item2);

            //calcualte grade from distance and altitude deltas
            _rawGrades = CalculateGrade(_distanceDeltas, _altitudeDeltas);

            //smoothing the grade is effectively the same a weighted grading the altitude, but simpler
            //we have to smooth the grade, as the cost is a polynomial of the grade, so it's too late to smooth then; any errors will be magnified 
            if (Options.Instance.SmoothingType == TimeSeries.SmoothingOptions.AverageWindow)
                _smoothedGrades = TimeSeries.WindowSmoothed(_rawGrades, Options.Instance.SmoothingWindow);
            else if (Options.Instance.SmoothingType == TimeSeries.SmoothingOptions.SimpleExponential)
                _smoothedGrades = TimeSeries.SimpleExponentialSmoothed(_rawGrades, Options.Instance.SmoothingWindow);
            else
                _smoothedGrades = _rawGrades;

            _costs = CalculateCost(_smoothedGrades);

            _gradeAdjustedDistance = CalculateGradeAdjustedDistance(_distanceDeltas, _costs);
        }
        */
        //private TimeDataTuple DistanceOrSpeed { get; set; }
        //private TimeDataTuple Altitudes { get; set; }
        private AlignedTimeSeries Aligned { get; set; }
        private float[] _distanceDeltas;
        private float[] _altitudeDeltas;
        private float[] _rawGrades;
        private float[] _smoothedGrades;
        private float[] _costs;
        private float[] _gradeAdjustedDistance;

        public TimeValueList GetGradeAdjustedDistance()
        {
            TimeValueList gradeAdjustedDistance = new TimeValueList(Aligned.Time, _gradeAdjustedDistance);

            if(gradeAdjustedDistance.Times.Length != gradeAdjustedDistance.Values.Length)
            {
                throw new Exception($"GetGradeAdjustedDistance times {gradeAdjustedDistance.Times.Length} and values {gradeAdjustedDistance.Values.Length} don't match counts");
            }

            return gradeAdjustedDistance; 
        }


        private float[] CalculateDelta(float[] input)
        {
            
            float[] deltas = new float[input.Length];

            float last = input[0];
            deltas[0] = 0; //no previous value, delta has to be zero
            for(int i=1; i < input.Length; i++) //note starting from one, as the first element is zero
            {
                float change = input[i] - last;
                deltas[i] = change;
                last = input[i];
            }
            return deltas;
        }

        private float[] CalculateGrade(float[] dd, float[] ad)
        {

            float[] grades = new float[dd.Length];

            grades[0] = 0; //no previous value, grade of the first point has to be zero
            for (int i = 1; i < dd.Length; i++) //note starting from one, as the first element is zero
            {
                float distanceDelta = dd[i];
                float altitudeDelta = ad[i];
                float grade;
                if (distanceDelta == 0)
                {
                    grade = 0;
                }
                else
                {
                    grade = altitudeDelta / distanceDelta;

                    //constrain slope before smoothing in case of discontinuities
                    if (grade > Options.Instance.MaxSlope)
                        grade = (float)Options.Instance.MaxSlope;

                    if (grade < Options.Instance.MinSlope)
                        grade = (float)Options.Instance.MinSlope;
                }
                grades[i] = grade;
                if (grade != 0 && !double.IsNormal(grade))
                {
                    Logging.Instance.Log(string.Format("invalid grade value {0} from altitude {1} and distance {2} at offset {3}", grade, altitudeDelta, distanceDelta, i));
                }
            }
            return grades;
        }
        private float[] CalculateCost(float[] grades)
        {

            float[] costs = new float[grades.Length];

            for (int i = 0; i < grades.Length; i++)
            {
                float grade = grades[i];


                //=POWER(A2,2)*15.14+A2*2.896+1.0098
                //double cost = Math.Pow(slope, 2) * 15.14 + slope * 2.896 + 1.0098;
                float cost = (float)(Math.Pow(grade, 2) * Options.Instance.GradeAdjustmentX2 + grade * Options.Instance.GradeAdjustmentX + Options.Instance.GradeAdjustmentOffset);

                costs[i] = cost;
                if (cost != 0 && !double.IsNormal(cost))
                {
                    Logging.Instance.Log(string.Format("invalid cost value {0} from grade {1} at offset {2}", cost, grade, i));
                }
            }
            return costs;
        }

        private float[] CalculateGradeAdjustedDistance(float[] dd, float[] costs)
        {

            float[] gradeAdjustedDistance = new float[dd.Length];
            float currentDistance = 0;
            for (int i = 0; i < dd.Length; i++)
            {
                float distanceDelta = dd[i];
                float costAdjustment = costs[i];
                float adjustedDistance = distanceDelta * costAdjustment;
                currentDistance += adjustedDistance;
                gradeAdjustedDistance[i] = currentDistance;
            }
            return gradeAdjustedDistance;
        }


    }
}
