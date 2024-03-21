using MemoryPack;
using FellrnrTrainingAnalysis.Utils;
using System.ComponentModel;
using System.Xml.Linq;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]
    public partial class Day : Extensible
    {
        public Day(DateTime date)
        {
            Date = date.Date; //be paranoid about not introducing times
        }
        [MemoryPackInclude]
        public DateTime Date { get; set; }

        public override Utils.DateTimeTree Id() { return new DateTimeTree(Date, DateTimeTree.DateTreeType.Day); }

        [MemoryPackInclude]
        private List<Activity>? _activities = null;

        [MemoryPackIgnore]
        public IReadOnlyList<Activity> Activities { get { if (_activities == null) { return new List<Activity>().AsReadOnly(); } else { return _activities.AsReadOnly(); } } }

        public void AddActivity(Activity activity)
        {
            if (_activities == null)
                _activities = new List<Activity>();
            if(!_activities.Contains(activity))
                _activities.Add(activity);
        }

        public override void Recalculate(int forceCount, bool forceJustMe, BackgroundWorker? worker = null)
        {
            bool force = false;
            if (forceCount > LastForceCount || forceJustMe) { LastForceCount = forceCount; force = true; }

            if (force)
                base.Clean();
        }

        public const string WeightTag = "Weight";
        public const string RestingHeartRateTag = "Resting Heart Rate";

        public override string ToString()
        {
            string s = _activities == null ? "null" : $"#{_activities.Count}";
            string d = _activities == null || _activities.Count == 0 ? "" : $", First {_activities.First()}";
            return string.Format($"Day: [Date {Date}, Activites {s}{d}]");
        }

    }
}
