using Dynastream.Fit;
using FellrnrTrainingAnalysis.Model;
using FellrnrTrainingAnalysis.Utils;
using GoogleApi.Entities.Common.Enums;
using MPFitLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static FellrnrTrainingAnalysis.Model.PowerDistributionCurve;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace FellrnrTrainingAnalysis.Action
{
    public abstract class PdmFit 
    {
        public PdmFit(PdmModel model)
        {
            Model = model;
        }

        public void DeriveCPParameters(TimeValueList bestCurves)
        {
            BestCurves = bestCurves;
            TimesForCurve = PowerDistributionCurve.GetTimes(BestCurves);
            DataForCurve = BestCurves.Values;
            DeriveCPParameters();
        }

        protected abstract void DeriveCPParameters();
        public virtual void Reset()
        {
            Model.Reset();
        }

        public abstract PdmFit DeepCopy();

        protected TimeValueList? BestCurves { get; set; }
        protected int[]? TimesForCurve { get; set; }
        protected float[]? DataForCurve { get; set; }

        public PdmModel Model { get; set; }

    }
    public class PdmFitLinearRegression : PdmFit
    {
        public PdmFitLinearRegression(PdmModel model) : base(model)
        {
            if(model is not PdmModel2Param) { throw new Exception("Sorry, Linear Regression only support 2 parameter model");  }
        }

        public override PdmFit DeepCopy()
        {
            return new PdmFitLinearRegression(Model.DeepCopy());
        }

        protected override void DeriveCPParameters()
        {
            List<float> t = new List<float>();
            List<float> jules = new List<float>();

            if (TimesForCurve == null || TimesForCurve.Length < 1200)
                return;

            for (int i = 0; i < BestCurves!.Length; i++)
            {
                if (TimesForCurve![i] > 120 && TimesForCurve![i] <= 1200)
                {
                    jules.Add(DataForCurve![i] * TimesForCurve![i]);
                    t.Add(TimesForCurve![i]);
                }
            }

            LinearRegression? lr = LinearRegression.EvaluateLinearRegression(t.ToArray(), jules.ToArray(), ignoreZerosX: false, ignoreZerosY: false);

            if (lr == null)
            {
                Logging.Instance.Error("Linear Regression failed");
                throw new Exception("Linear Regression failed");
            }


            //Linear regression can only set CP and WPrime
            double newCP = (double)lr.Slope;
            double newWPrime = (double)lr.YIntercept;
            Model.SetCpWPrime(newCP, newWPrime);
        }
    }

    public class PdmFitLeastSquares : PdmFit
    {
        public PdmFitLeastSquares(PdmModel model, PdmExtract? extract) : base(model)
        {
            this.Extract = extract;
        }

        public PdmFitLeastSquares(PdmModel model) : base(model)
        {
        }

        public override PdmFit DeepCopy()
        {
            return new PdmFitLeastSquares(Model.DeepCopy(), Extract);
        }

        PdmExtract? Extract = null;
        protected override void DeriveCPParameters()
        {
            if (Extract == null)
                this.Extract = Model.GetDefaultExtract(BestCurves!);

            if (Extract.Data.Length == 0)
                return;
            double[] dataDouble = Array.ConvertAll(Extract.Data, x => (double)x);
            double[] timeDouble = Array.ConvertAll(Extract.Times, x => (double)x);
            double[] estimatedY = new double[dataDouble.Length];


            CustomUserVariable v = new CustomUserVariable(timeData: timeDouble, powerData: dataDouble, evaluate: Model.EstimatePowerAtTimeFromLeastSquaresParameters);

            double[] workingParameters = Model.GetDefaultLeastSquaresParameters();

            mp_result result = new mp_result(workingParameters.Length);

            //public static int Solve(mp_func funct, int m, int npar, double[] xall, mp_par[] pars, mp_config config, object prv, ref mp_result result, TextWriter logger = null)
            int status = MPFit.Solve(Model.EstimatePowerCurveFromLeastSquaresParameters,
                dataDouble.Length, //length of data
                npar: workingParameters.Length, //number of fit parameters
                xall: workingParameters, //array of n initial parameter values upon return, contains adjusted parameter values
                pars: null, //array of npar structures specifying constraints; or 0 (null pointer) for unconstrained fitting
                config: null,
                prv: v, //any private user data which is to be passed directly to funct without modification by MPFit.Solve.
                ref result); //structure, which upon return, contains the results of the fit. 

            Model.SetValuesFromLeastSquaresParameters(workingParameters);
        }

    }
    public class PdmFitEnvelope : PdmFit
    {

        //default anaerobic is 2:00 to 4:00, aerobic 10:00 to 20:00

        public PdmFitEnvelope(PdmModel model,
                              int anaerobicInterval1 = 120,
                              int anaerobicInterval2 = 240,
                              int aerobicInterval1 = 600,
                              int aerobicInterval2 = 1200) : base(model)
        {
            AnaerobicInterval1 = anaerobicInterval1;
            AnaerobicInterval2 = anaerobicInterval2;
            AerobicInterval1 = aerobicInterval1;
            AerobicInterval2 = aerobicInterval2;
        }

        int AnaerobicInterval1, AnaerobicInterval2, AerobicInterval1, AerobicInterval2;

        public override PdmFit DeepCopy()
        {
            return new PdmFitEnvelope(Model.DeepCopy(), AnaerobicInterval1, AnaerobicInterval2, AerobicInterval1, AerobicInterval2);
        }

        PdmExtract? Extract;
        protected override void DeriveCPParameters()
        {
            if (Extract == null)
                this.Extract = Model.GetDefaultExtract(BestCurves!);

            if (Extract.Data.Length == 0)
                return;
            float[] data= Extract.Data;
            int[] times = Extract.Times;

            int t1 = AnaerobicInterval1;
            int t2 = AnaerobicInterval2;

            int t3 = AerobicInterval1;
            int t4 = Math.Min(AerobicInterval2, times.Last());


            //we need at least the start of the aerobic phase
            if (AerobicInterval1 > times.Last())
                return;

            // bounds of these time values in the data
            int i1=0, i2 = 0, i3 = 0, i4 = 0;

            for(int i=0; i<times.Length; i++)
            {
                if (times[i] >= t1 && i1 == 0)
                    i1 = i;
                if (times[i] >= t2 && i2 == 0)
                    i2 = i;
                if (times[i] >= t3 && i3 == 0)
                    i3 = i;
                if (times[i] >= t4 && i4 == 0)
                    i4 = i;
            }
            Model.ModelPowerFromEnvelope(times, data, i1, i2, i3, i4);
        }

    }
    public class CustomUserVariable
    {
        public double[] TimeData;
        public double[] PowerData;
        public delegate double Evaluation(double time, double[] updatedParameters);
        public Evaluation Evaluate;

        public CustomUserVariable(double[] timeData, double[] powerData, Evaluation evaluate)
        {
            TimeData = timeData;
            PowerData = powerData;
            Evaluate = evaluate;
        }
    }

    public abstract class PdmModel
    {

        protected PdmModel()
        {
        }

        public abstract void SetValuesFromLeastSquaresParameters(double[] LeastSquaresParameters);

        public abstract double[] GetDefaultLeastSquaresParameters();

        public abstract void SetCpWPrime(double newCP, double newWPrime);

        public virtual void Reset()
        {
            WPrime = null;
            CP = null;
            FTP = null;
            PMax = null;
            Tau = null;
            T0 = null;
            MapTimeToValue = null;
        }

        public abstract PdmModel DeepCopy();


        public virtual double? WPrime { get; set; } = null;
        public virtual double? CP { get; set; } = null;
        public virtual double? FTP { get; set; } = null;
        public virtual double? PMax { get; set; } = null;
        public virtual double? Tau { get; set; } = null;

        public virtual double? T0 { get; set; } = null;

        public Dictionary<int, double>? MapTimeToValue { get; set; } = null;

        public int EstimatePowerCurveFromLeastSquaresParameters(double[] newParams, double[] fvec, IList<double>[] dvec, object prv)
        {
            CustomUserVariable v = (CustomUserVariable)prv; //"private" data

            //calculate the difference between the actual power and the expected power
            for (int i = 0; i < fvec.Length; i++)
                fvec[i] = v.PowerData[i] - this.EstimatePowerAtTimeFromLeastSquaresParameters(v.TimeData[i], newParams); //called "f" in golden cheetah 

            return 0;
        }

        public abstract double EstimatePowerAtTimeFromLeastSquaresParameters(double time, double[] updatedParameters); //called "f" in golden cheetah 

        public abstract double ModelledPowerAtTime(double time); //called "y" in golden cheetah as it's used to plot the modelled curve


        public void ModelPowerFromEnvelope(int[] TimesForCurve, float[] DataForCurve, int i1, int i2, int i3, int i4)
        {


            // initial estimate of tau
            if (Tau == null || Tau == 0) Tau = 1;

            // initial estimate of cp (if not already available)
            if (CP == null || CP == 0) CP = 300;

            // initial estimate of t0: start small to maximize sensitivity to data
            T0 = 0;

            // lower bound on tau
            const double tau_min = 0.5;

            // convergence delta for tau
            const double tau_delta_max = 1e-4;
            const double t0_delta_max = 1e-4;

            // previous loop value of tau and t0
            double tau_prev;
            double t0_prev;

            // maximum number of loops
            const int max_loops = 100;

            Tuple<int, double>? cpCherry = null;
            Tuple<int, double>? tauCherry = null;
            // loop to convergence
            int iteration = 0;
            //bool changed;
            int index;
            double value;
            do
            {
                // bounds check, don't go on for ever
                if (iteration++ > max_loops) break;

                // clear cherries
                //MapTimeToValue.Clear();


                // record the previous version of tau, for convergence
                tau_prev = (double)Tau;
                t0_prev = (double)T0;

                // estimate cp, given tau
                int i;
                CP = 0;
                //changed = false;
                index = 0;
                value = 0;
                for (i = i3; i < i4; i++)
                {
                    double cpn = DataForCurve[i] / (1 + Tau.Value / (T0.Value + TimesForCurve[i] / 60.0));
                    if (CP < cpn)
                    {
                        CP = cpn;
                        index = TimesForCurve[i];
                        value = DataForCurve[i];
                        cpCherry = new Tuple<int, double>(index, value);
                        //changed = true;
                    }
                }
                //if (changed) MapTimeToValue.Add(index, value);


                // if cp = 0; no valid data; give up
                if (CP == 0.0)
                    return;

                // estimate tau, given cp
                Tau = tau_min;
                //changed = false;
                for (i = i1; i <= i2; i++)
                {
                    double taun = (DataForCurve[i] / CP.Value - 1) * (TimesForCurve[i] / 60.0 + T0.Value) - T0.Value;
                    if (Tau < taun)
                    {
                        //changed = true;
                        index = TimesForCurve[i];
                        value = DataForCurve[i];
                        tauCherry = new Tuple<int, double>(index, value);
                        Tau = taun;
                    }
                }
                //if (changed) MapTimeToValue.Add(index, value);

                // estimate t0 - but only for veloclinic/3parm cp
                // where model is not CP2 
                UpdateT0ForEnvelope(DataForCurve);

            } while ((Math.Abs(Tau.Value - tau_prev) > tau_delta_max) || (Math.Abs(Tau.Value - t0_prev) > t0_delta_max));


            MapTimeToValue = new Dictionary<int, double>();
            if(cpCherry != null)
                MapTimeToValue.Add(cpCherry.Item1, cpCherry.Item2);
            if (tauCherry != null)
                MapTimeToValue.Add(tauCherry.Item1, tauCherry.Item2);

            if (CP < 0)
            {
                Logging.Instance.Debug($"Oops, ModelPowerFromEnvelope, CP is negative, {CP}");
            }

        }

        protected abstract void UpdateT0ForEnvelope(float[] DataForCurve);

        public abstract PdmExtract GetDefaultExtract(TimeValueList best);

    }

    public class PdmModel2Param : PdmModel
    {
        public override void SetValuesFromLeastSquaresParameters(double[] LeastSquaresParameters)
        {
            // set the model parameters with the values from the fit
            CP = LeastSquaresParameters[0];
            WPrime = LeastSquaresParameters[1];
            Tau = WPrime / (CP * 60);
            if(CP < 0)
            {
                Logging.Instance.Debug($"Oops, PdmModel2Param/Setvalues, CP is negative, {CP}");
            }
        }

        public override double[] GetDefaultLeastSquaresParameters()
        {
            return new double[] { 250, 18000 };
        }

        public override void SetCpWPrime(double newCP, double newWPrime)
        {
            CP = newCP;
            WPrime = newWPrime;
            Tau = WPrime / (CP * 60);
            if (CP < 0)
            {
                Logging.Instance.Debug($"Oops, PdmModel2Param/SetCpWPrime, CP is negative, {CP}");
            }
        }

        public override PdmModel DeepCopy()
        {
            PdmModel2Param result = new PdmModel2Param();
            result.CP = CP;
            result.WPrime = WPrime;
            result.Tau = Tau;
            result.PMax = PMax;
            result.FTP = FTP;
            result.T0 = T0;
            return result;
        }
        public override double EstimatePowerAtTimeFromLeastSquaresParameters(double time, double[] updatedParameters)
        {
            double updatedCP = updatedParameters[0];
            double updatedTau = updatedParameters[1];
            double power = updatedCP + (updatedTau / time);

            return power;
            //double f(double t, const double* parms) {
            //    return parms[0] + (parms[1] / t);
            //}
        }

        public override double ModelledPowerAtTime(double time)
        {
            // classic model - W' / t + CP
            if(CP == null || Tau == null) return 0; 

            double cp = (double)CP;
            double tau = (double)Tau;
            return (cp * tau * 60) / time + cp;
        }

        public override PdmExtract GetDefaultExtract(TimeValueList best)
        {
            return new PdmExtract(best, 120, 1200);
        }

        protected override void UpdateT0ForEnvelope(float[] DataForCurve)
        {
        }
    }

    public class PdmModel3Param : PdmModel
    {
        public PdmModel3Param(bool modelDecayForLeastSquares = true)
        {
            ModelDecay = modelDecayForLeastSquares;
        }

        public bool ModelDecay { get; set; } = false; //taken from GC, don't reset

        public override void SetValuesFromLeastSquaresParameters(double[] LeastSquaresParameters)
        {
            // set the model parameters with the values from the fit
            //this->cp = parms[0];
            //this->tau = parms[1] / (cp * 60.00);
            //double pmax = parms[0] + (parms[1] / (1 + parms[2]));
            //this->t0 = this->tau / (pmax / this->cp - 1) - 1 / 60.0;
            if (LeastSquaresParameters[0] < 0)
            {
                Logging.Instance.Debug($"Oops, PdmModel3Param, CP is negative, {LeastSquaresParameters[0]}");
                return;
            }
            CP = LeastSquaresParameters[0];
            Tau = LeastSquaresParameters[1] / (CP * 60.00);
            
            double pmax_local = LeastSquaresParameters[0] + (LeastSquaresParameters[1] / (1 + LeastSquaresParameters[2]));
            T0 = Tau / (pmax_local / CP - 1) - 1 / 60.0;
        }



        public override double[] GetDefaultLeastSquaresParameters()
        {
            //CP, W', K
            return new double[] { 250, 18000, 32 };
        }

        public override void SetCpWPrime(double newCP, double newWPrime)
        {
            throw new NotSupportedException("3 parameter model doesn't support direct CP/W' setting (no linear regression)");
        }

        public override double? WPrime { get { return CP == null || Tau == null ? null : CP * Tau * 60.0f; } }

        public override PdmModel DeepCopy()
        {
            PdmModel3Param result = new PdmModel3Param();
            result.CP = CP;
            result.WPrime = WPrime;
            result.Tau = Tau;
            result.PMax = PMax;
            result.FTP = FTP;
            result.T0 = T0;

            result.ModelDecay = ModelDecay;
            return result;
        }

        public override double EstimatePowerAtTimeFromLeastSquaresParameters(double time, double[] updatedParameters)
        {
            //from GC "f"
            //double cp = parms[0];
            //double w = parms[1];
            //double k = parms[2];

            //return cp + (w / (t + k));

            double cp = updatedParameters[0];
            double w = updatedParameters[1];
            double k = updatedParameters[2];

            double power = cp + (w / (time + k));

            return power;
        }

        public override double ModelledPowerAtTime(double time)
        {

            if (CP == null || Tau == null || T0 == null) return 0;

            time += 1;

            double cp = (double)CP;
            double tau = (double)Tau;
            double t0 = (double)T0;

            // decay value (75% at 10 hrs)
            double cpdecay = 1.0;
            double wdecay = 1.0;


            // just use a constant for now - it modifies CP/W' so should adjust to
            // athlete without needing to be fitted (?)
            if (ModelDecay)
            {
                cpdecay = 2.0 - (1.0 / Math.Exp(-0.000009 * time));
                wdecay = 2.0 - (1.0 / Math.Exp(-0.000025 * time));
            }
            // typical values: CP=285.355547 tau=0.500000 t0=0.381158

            // classic model - W' / t + CP
            //return cp * cpdecay * (1.00 + (tau * wdecay) / (time / 60) + t0;

            double power = (cp * cpdecay) * (1.0 + (tau * wdecay) / (((time / 60) + T0.Value)));

            return power;

        }

        public override PdmExtract GetDefaultExtract(TimeValueList best)
        {
            return new PdmExtract(best, 0, 1200);
        }


        protected override void UpdateT0ForEnvelope(float[] DataForCurve)
        {
            T0 = Tau / (DataForCurve[1] / CP!.Value - 1) - 1 / 60.0;
        }

    }


    public class PdmExtract
    {
        public PdmExtract(TimeValueList bestCurves, int min, int max)
        {
            List<float> d = new List<float>();
            List<int> t = new List<int>();

            int[] times = PowerDistributionCurve.GetTimes(bestCurves);
            float[] data = bestCurves.Values;

            for (int i = 0; i < bestCurves.Length; i++)
            {
                if (times[i] > min && times[i] <= max)
                {
                    d.Add(data[i]);
                    t.Add(times[i]);
                }
            }

            Data = d.ToArray();
            Times = t.ToArray();
        }

        public PdmExtract(TimeValueList BestCurves)
        {
            int[] times = PowerDistributionCurve.GetTimes(BestCurves);
            float[] data = BestCurves.Values;

            Data = data.ToArray();
            Times = times.ToArray();
        }

        public float[] Data { get; set; }
        public int[] Times { get; set; }
    }



    /*
    public class PowerDurationModeller3Parameter : PowerDurationModeller
    {

        private bool modelDecay = false;

        public PowerDurationModeller3Parameter(TimeValueList bestCurves, FitType fit) : base(bestCurves, fit, 120, 200, 720, 1200, 0, 0, 0, 0)
        {
        }

        protected override double y(double t)
        {
            // don't start at zero !
            t += (!minutes ? 1.00f : 1 / 60.00f);

            // adjust to seconds
            if (minutes) t *= 60.00f;

            // decay value (75% at 10 hrs)
            double cpdecay = 1.0;
            double wdecay = 1.0;

            // just use a constant for now - it modifies CP/W' so should adjust to
            // athlete without needing to be fitted (?)
            if (modelDecay)
            {
                cpdecay = 2.0 - (1.0 / Math.Exp(-0.000009 * t));
                wdecay = 2.0 - (1.0 / Math.Exp(-0.000025 * t));
            }
            // typical values: CP=285.355547 tau=0.500000 t0=0.381158

            // classic model - W' / t + CP
            return (cp * cpdecay) * (1 + (tau * wdecay) / (t / 60.0) + t0);
        }


    }
    */
    /*   public abstract class PowerDurationModeller
       {
           //based on the logic of Golden Cheetah
           //Look at PDModel.cpp


           private TimeValueList BestCurves { get; set; }
           private double[] TimesForCurve { get; set; }
           private double[] DataForCurve { get; set; }

           public enum FitType
           {
               //Envelope = 0,                 // envelope fit
               //LeastSquares = 1,             // uses Levenberg-Marquardt Damped Least Squares
               LinearRegression = 2         // using slope and intercept of linear regression
           };




           public double WPrime { get; set; }      // return estimated W'
           public double CP { get; set; }      // return CP
           public double FTP { get; set; }      // return FTP
           public double PMax { get; set;  }      // return PMax

           protected double sanI1, sanI2, anI1, anI2, aeI1, aeI2, laeI1, laeI2;

           // standard CP derived values - set when deriveCPParameters is called
           protected double cp, tau, t0; // CP model parameters

           public PowerDurationModeller(TimeValueList bestCurves,
                                        FitType fit,
                                        double sanI1,
                                        double sanI2,
                                        double anI1,
                                        double anI2,
                                        double aeI1,
                                        double aeI2,
                                        double laeI1,
                                        double laeI2)
           {
               BestCurves = bestCurves;
               TimesForCurve = PowerDistributionCurve.GetTimes(BestCurves);
               DataForCurve = BestCurves.Values;
               this.sanI1 = sanI1;
               this.sanI2 = sanI2;
               this.anI1 = anI1;
               this.anI2 = anI2;
               this.aeI1 = aeI1;
               this.aeI2 = aeI2;
               this.laeI1 = laeI1;
               this.laeI2 = laeI2;
           }

           protected List<double> Data { get; set; }
           protected List<double> Times { get; set; }


           public double vo2max() { return (double)(10.8f * y(300) + 7f); }

           protected bool minutes = false;

           protected abstract double y(double t);

       }
    */

}
