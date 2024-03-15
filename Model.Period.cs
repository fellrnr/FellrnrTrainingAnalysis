using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FellrnrTrainingAnalysis.Model
{
    public abstract class Period
    {
        public Period()
        {
        }

        public abstract bool IsWithinPeriod(DateTime sample, DateTime target);

        public abstract string FullName { get; }
        public abstract string ShortName { get; }
        public abstract int? ApproxDays { get; }

        //public static List<Period> DefaultDisplayPeriods = new List<Period> { new PeriodRolling(0, 0, 6), new PeriodRolling(0, 0, 7), new PeriodRolling(0, 1, 0), new PeriodRolling(1, 0, 0), new PeriodYearToDate() };
        public static List<Period> DefaultDisplayPeriods = new List<Period> { new PeriodRolling(0, 0, 7), new PeriodRolling(0, 0, 30), new PeriodRolling(1, 0, 0), new PeriodYearToDate(), new PeriodLifetime() };
        public static List<Period> DefaultEmailPeriods = new List<Period> { new PeriodRolling(0, 0, 7), new PeriodRolling(0, 0, 30), new PeriodRolling(1, 0, 0), new PeriodYearToDate(), new PeriodLifetime() };
        public static List<Period> DefaultStorePeriods = new List<Period> { new PeriodRolling(0, 0, 7), new PeriodRolling(0, 0, 30), new PeriodRolling(1, 0, 0), new PeriodYearToDate(), new PeriodLifetime() };
    }

    internal class PeriodRolling : Period
    {
        public PeriodRolling(int years, int months, int days)
        {
            Years = years;
            Months = months;
            Days = days;
        }

        //typically activity date and current date as parameters
        public override bool IsWithinPeriod(DateTime sample, DateTime target)
        {
            target = target.Date;
            sample = sample.Date;
            if (sample > target) return false; //past the target date (in the future)

            DateTime yearOffset = Years > 0 ? sample.AddYears(Years) : sample;
            DateTime monthOffset = Months > 0 ? yearOffset.AddMonths(Months) : yearOffset;
            DateTime dayOffset = Days > 0 ? monthOffset.AddDays(Days) : monthOffset;

            if (dayOffset <= target) return false; //too far in the past

            return true;
        }

        public override string FullName { get { return (Years > 0 ? $"{Years} Years" : "") + (Months > 0 ? $"{Months} Months" : "") + (Days > 0 ? $"{Days} Days" : ""); } }
        public override string ShortName { get { return (Years > 0 ? $"{Years}Y" : "") + (Months > 0 ? $"{Months}M" : "") + (Days > 0 ? $"{Days}D" : ""); } }

        private int Years { get; set; }
        private int Months { get; set; }
        private int Days { get; set; }

        public override int? ApproxDays { get { return Years * 365 + Months * 30 + Days; } }

    }

    internal class PeriodYearToDate : Period
    {
        public PeriodYearToDate()
        {
        }

        public override bool IsWithinPeriod(DateTime sample, DateTime target)
        {
            target = target.Date;
            sample = sample.Date;
            if (sample > target) return false; //past the target date (in the future)

            if (sample.Year != target.Year) return false; //not this year

            return true;
        }

        public override string FullName { get { return "YTD"; } }
        public override string ShortName { get { return "YTD"; } }


        public override int? ApproxDays { get { return DateTime.Now.DayOfYear; } }

    }

    internal class PeriodLifetime : Period
    {
        public PeriodLifetime()
        {
        }

        public override bool IsWithinPeriod(DateTime sample, DateTime target)
        {
            return true;
        }

        public override string FullName { get { return "All"; } }
        public override string ShortName { get { return "All"; } }


        public override int? ApproxDays { get { return null; } }

    }

}
