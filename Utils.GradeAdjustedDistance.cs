using FellrnrTrainingAnalysis.Model;

namespace FellrnrTrainingAnalysis.Utils
{
    public class GradeAdjustedDistance
    {

        public GradeAdjustedDistance(TimeValueList inclineData,
                                    TimeValueList speedData,
                                    float? gradeAdjustmentX2 = null, 
                                    float? gradeAdjustmentX3 = null, 
                                    float? gradeAdjustmentX4 = null, 
                                    float? gradeAdjustmentX5 = null, 
                                    float? gradeAdjustmentX = null,
                                    float? gradeAdjustmentFactor = null,
                                    float? gradeAdjustmentOffset= null)
        {
            Logging.Instance.ContinueAccumulator($"Utils.GradeAdjustedDistance.GradeAdjustedDistance");
            _inclineData = inclineData.Values;
            if (_inclineData.Length < 2)
                throw new ArgumentException("need at least two inclines to calculate GAP");

            //calcualte grade from distance and altitude deltas
            _rawGrades = inclineData.Values;

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

            _gradeAdjustedPace = CalculateGradeAdjustedPace(speedData, _costs);

            Logging.Instance.PauseAccumulator($"Utils.GradeAdjustedDistance.GradeAdjustedDistance");
        }


        private float[] _inclineData;
        private float[] _rawGrades;
        private float[] _smoothedGrades;
        private float[] _costs;

        private float _gradeAdjustedDistance;
        TimeValueList _gradeAdjustedPace;


        private float GradeAdjustmentX5 { get; set; }
        private float GradeAdjustmentX4 { get; set; }
        private float GradeAdjustmentX3 { get; set; }
        private float GradeAdjustmentX2 { get; set; }
        private float GradeAdjustmentX { get; set; }
        private float GradeAdjustmentFactor { get; set; }
        private float GradeAdjustmentOffset { get; set; }

        public float GetGradeAdjustedDistance()
        {
            return _gradeAdjustedDistance;
        }

        public TimeValueList GetGradeAdjustedPace()
        {
            return _gradeAdjustedPace;
        }

        private float[] CalculateCost(float[] grades)
        {
            Logging.Instance.ContinueAccumulator($"Utils.GradeAdjustedDistance.CalculateCost");

            float[] costs = new float[grades.Length];

            for (int i = 0; i < grades.Length; i++)
            {
                float grade = grades[i];


                //=POWER(A2,2)*15.14+A2*2.896+1.0098
                //float cost = (float)Math.Pow(slope, 2) * 15.14 + slope * 2.896 + 1.0098;
                //float cost = (float)((float)Math.Pow(grade, 2) * GradeAdjustmentX2 + grade * GradeAdjustmentX);
                //
                float costAbs;
                if (GradeAdjustmentX5 != 0 && GradeAdjustmentX4 != 0 && GradeAdjustmentX3 != 0)
                {
                    costAbs = (float)Math.Pow(grade, 5) * GradeAdjustmentX5 +
                        (float)Math.Pow(grade, 4) * GradeAdjustmentX4 +
                        (float)Math.Pow(grade, 3) * GradeAdjustmentX3 +
                        (float)Math.Pow(grade, 2) * GradeAdjustmentX2 +
                        grade * GradeAdjustmentX +
                        GradeAdjustmentOffset;
                }
                else
                {
                    costAbs = grade * grade * GradeAdjustmentX2 +
                              grade * GradeAdjustmentX +
                              GradeAdjustmentOffset;
                }

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
            Logging.Instance.PauseAccumulator($"Utils.GradeAdjustedDistance.CalculateCost");
            return costs;
        }
        private bool firstError = true;

        private TimeValueList CalculateGradeAdjustedPace(TimeValueList speedData, float[] costs)
        {

            _gradeAdjustedDistance = 0;
            int length = Math.Min(speedData.Length, costs.Length);
            float[] speeds = speedData.Values;

            float[] gradeAdjustedPace = new float[length];
            for (int i = 0; i < length; i++)
            {
                float speed = speeds[i];
                float costAdjustment = costs[i];
                float adjustedSpeed = speed * costAdjustment;
                gradeAdjustedPace[i] = adjustedSpeed;

                _gradeAdjustedDistance += adjustedSpeed; //each entry is a second, speed is m/s, so distance is the speed for this second
            }
            return new TimeValueList(gradeAdjustedPace);
        }


    }
}
