using FellrnrTrainingAnalysis.Model;

namespace FellrnrTrainingAnalysis.Utils
{
    public class GradeAdjustedDistance
    {

        public GradeAdjustedDistance(AlignedTimeSeries aligned, 
                                    float? gradeAdjustmentX2 = null, 
                                    float? gradeAdjustmentX3 = null, 
                                    float? gradeAdjustmentX4 = null, 
                                    float? gradeAdjustmentX5 = null, 
                                    float? gradeAdjustmentX = null,
                                    float? gradeAdjustmentFactor = null,
                                    float? gradeAdjustmentOffset= null)
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
            if (Options.Instance.GADSmoothingType == TimeSeriesUtils.SmoothingOptions.AverageWindow)
                _smoothedGrades = TimeSeriesUtils.WindowSmoothed(_rawGrades, Options.Instance.GADSmoothingWindow);
            else if (Options.Instance.GADSmoothingType == TimeSeriesUtils.SmoothingOptions.SimpleExponential)
                _smoothedGrades = TimeSeriesUtils.SimpleExponentialSmoothed(_rawGrades, Options.Instance.GADSmoothingWindow);
            else
                _smoothedGrades = _rawGrades;


            if (gradeAdjustmentX != null)
                GradeAdjustmentX = gradeAdjustmentX.Value;
            else
                GradeAdjustmentX = Options.Instance.GradeAdjustmentX;

            if (gradeAdjustmentX2 != null)
                GradeAdjustmentX2 = gradeAdjustmentX2.Value;
            else
                GradeAdjustmentX2 = Options.Instance.GradeAdjustmentX2;

            if (gradeAdjustmentX3 != null)
                GradeAdjustmentX3 = gradeAdjustmentX3.Value;
            else
                GradeAdjustmentX3 = Options.Instance.GradeAdjustmentX3;

            if (gradeAdjustmentX4 != null)
                GradeAdjustmentX4 = gradeAdjustmentX4.Value;
            else
                GradeAdjustmentX4 = Options.Instance.GradeAdjustmentX4;

            if (gradeAdjustmentX5 != null)
                GradeAdjustmentX5 = gradeAdjustmentX5.Value;
            else
                GradeAdjustmentX5 = Options.Instance.GradeAdjustmentX5;

            if (gradeAdjustmentFactor != null)
                GradeAdjustmentFactor = gradeAdjustmentFactor.Value;
            else
                GradeAdjustmentFactor = Options.Instance.GradeAdjustmentFactor;

            if (gradeAdjustmentOffset!= null)
                GradeAdjustmentOffset = gradeAdjustmentOffset.Value;
            else
                GradeAdjustmentOffset = Options.Instance.GradeAdjustmentOffset;

            //do the calculation

            _costs = CalculateCost(_smoothedGrades);

            _gradeAdjustedDistance = CalculateGradeAdjustedDistance(_distanceDeltas, _costs);

            _gradeAdjustedPace = CalculateDelta(_gradeAdjustedDistance);


            Logging.Instance.PauseAccumulator($"Utils.GradeAdjustedDistance.GradeAdjustedDistance");
        }


        private AlignedTimeSeries Aligned { get; set; }
        private float[] _distanceDeltas;
        private float[] _altitudeDeltas;
        private float[] _rawGrades;
        private float[] _smoothedGrades;
        private float[] _costs;
        private float[] _gradeAdjustedDistance;
        private float[] _gradeAdjustedPace;

        private float GradeAdjustmentX5 { get; set; }
        private float GradeAdjustmentX4 { get; set; }
        private float GradeAdjustmentX3 { get; set; }
        private float GradeAdjustmentX2 { get; set; }
        private float GradeAdjustmentX { get; set; }
        private float GradeAdjustmentFactor { get; set; }
        private float GradeAdjustmentOffset { get; set; }
        public TimeValueList GetGradeAdjustedDistance()
        {
            TimeValueList gradeAdjustedDistance = new TimeValueList(Aligned.Time, _gradeAdjustedDistance);

            if (gradeAdjustedDistance.Times.Length != gradeAdjustedDistance.Values.Length)
            {
                throw new Exception($"GetGradeAdjustedDistance times {gradeAdjustedDistance.Times.Length} and values {gradeAdjustedDistance.Values.Length} don't match counts");
            }

            return gradeAdjustedDistance;
        }

        public TimeValueList GetGradeAdjustedPace()
        {
            TimeValueList gradeAdjustedPace = new TimeValueList(Aligned.Time, _gradeAdjustedPace);

            return gradeAdjustedPace;
        }

        private float[] CalculateDelta(float[] input)
        {

            float[] deltas = new float[input.Length];

            float last = input[0];
            deltas[0] = 0; //no previous value, delta has to be zero
            for (int i = 1; i < input.Length; i++) //note starting from one, as the first element is zero
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
                if (grade != 0 && !float.IsNormal(grade))
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
                //float cost = (float)Math.Pow(slope, 2) * 15.14 + slope * 2.896 + 1.0098;
                //float cost = (float)((float)Math.Pow(grade, 2) * GradeAdjustmentX2 + grade * GradeAdjustmentX);
                //
                float costAbs = (float)Math.Pow(grade, 5) * GradeAdjustmentX5 + 
                    (float)Math.Pow(grade, 4) * GradeAdjustmentX4 + 
                    (float)Math.Pow(grade, 3) * GradeAdjustmentX3 + 
                    (float)Math.Pow(grade, 2) * GradeAdjustmentX2 + 
                    grade * GradeAdjustmentX + 
                    GradeAdjustmentOffset;

                float costRel = costAbs / GradeAdjustmentFactor;
                float cost = (float)costRel;
                costs[i] = cost;
                if (cost != 0 && !float.IsNormal(cost))
                {
                    costs[i] = 1;
                    if(firstError) Logging.Instance.Log(string.Format("invalid cost value {0} from grade {1} at offset {2}", cost, grade, i));
                    firstError = false; //only report once or we run out of memeory
                }
            }
            return costs;
        }
        private bool firstError = true;
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
