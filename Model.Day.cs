using MemoryPack;
using FellrnrTrainingAnalysis.Utils;

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
        public IReadOnlyList<Activity>? Activities { get { if (_activities == null) { return null; } else { return _activities.AsReadOnly(); } } }

        public void AddActivity(Activity activity)
        {
            if(_activities == null)
                _activities = new List<Activity>();
            _activities.Add(activity);
        }

    }
}
