using de.schumacher_bw.Strava.Endpoint;
using FellrnrTrainingAnalysis.Utils;
using MemoryPack;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]
    public partial class Database
    {

        public Database()
        {
            this.Athletes = new Dictionary<string, Athlete>();
            Athletes.Add("", new Athlete()); //TODO: Long term, handle the creation of a new athlete within the app, rather than loading. 
        }

        [MemoryPackInclude]
        public Dictionary<string, Athlete> Athletes { get; set; }

        [MemoryPackInclude]
        public List<Hill>? Hills { get; set; } = null;

        [MemoryPackIgnore]
        protected int LastForceCount = 0;

        public void MasterRecalculate(bool forceActivities, bool forceHills, bool forceGoals, BackgroundWorker? worker = null)
        {
            //we still want an unforced recalculation! 
            //if (forceActivities)
                RecalculateActivities(forceActivities, worker);
            //if (forceHills)
                RecalculateHills(forceHills, worker);
            //if (forceGoals)
                RecalculateGoals(forceGoals, worker);
        }

        //reapply the dynamic components, currently dyanamic data streams and goals
        private void RecalculateActivities(bool force, BackgroundWorker? worker = null)
        {
            Logging.Instance.TraceEntry("RecalculateActivities");



            //do this first to calculate today's CP
            int i = 0;
            List<Rolling> rollings = RollingFactory.GetPreRollings();
            if (worker != null) worker.ReportProgress(0, new Misc.ProgressReport($"Recalculate Pre Rolling Data ({rollings.Count})", rollings.Count));
            foreach (Rolling rolling in rollings)
            {
                rolling.Recalculate(this, force);
                if (worker != null) worker.ReportProgress(++i);
            }

            LastForceCount = force ? LastForceCount + 1 : LastForceCount;

            foreach (KeyValuePair<string, Athlete> kvp in Athletes)
            {
                Athlete athlete = kvp.Value;
                athlete.Recalculate(LastForceCount, false, worker);
            }

            //if (worker != null) worker.ReportProgress(0, new Misc.ProgressReport($"Recalculate Activities ({CurrentAthlete.Activities.Count})", CurrentAthlete.Activities.Count));
            //if (worker != null) worker.ReportProgress(++i);

            Logging.Instance.TraceLeave();
        }

        private void RecalculateGoals(bool force, BackgroundWorker? worker = null)
        {
            Logging.Instance.TraceEntry("RecalculateGoals");
            //It's important that calendar nodes don't accumulate goals, partly because of order, and partly because it doesn't make sense. To add all the days in a week's value for 30 running is meaningless 
            List<Cumulative> Cumulatives = CumulativeFactory.GetCumulatives();

            if (worker != null) worker.ReportProgress(0, new Misc.ProgressReport($"Recalculate Cumulatives ({Cumulatives.Count})", Cumulatives.Count));
            int i = 0;
            foreach (Cumulative Cumulative in Cumulatives)
            {
                Cumulative.UpdateActivityCumulatives(this, force);
                if (worker != null) worker.ReportProgress(++i);
            }

            i = 0;
            List<Rolling> rollings = RollingFactory.GetPostRollings();
            if (worker != null) worker.ReportProgress(0, new Misc.ProgressReport($"Recalculate Rolling Data ({rollings.Count})", rollings.Count));
            foreach (Rolling rolling in rollings)
            {
                rolling.Recalculate(this, force);
                if (worker != null) worker.ReportProgress(++i);
            }

            i = 0;
            List<CalculateFieldBase> calcdays = Model.CaclulateFieldFactory.Instance.DayCalculators;
            if (worker != null) worker.ReportProgress(0, new Misc.ProgressReport($"Recalculate Rolling Data ({CurrentAthlete.Days.Count})", CurrentAthlete.Days.Count));
            foreach (KeyValuePair<DateTime, Day> kvp in CurrentAthlete.Days)
            {
                Day day = kvp.Value;
                foreach (CalculateFieldBase calc in calcdays)
                {
                    calc.Recalculate(day, LastForceCount, false); //LastForceCount updated above in activity recalculate
                    if (worker != null) worker.ReportProgress(++i);
                }
            }
            Logging.Instance.TraceLeave();
        }

        private void RecalculateHills(bool force, BackgroundWorker? worker = null)
        {
            Logging.Instance.TraceEntry("RecalculateHills");

            if (force || Hills == null)
                Hills = Hill.Reload();


            if (!Options.Instance.DebugBlockParallel)
            {
                CurrentAthlete.Activities
                    .AsParallel()
                    .ForAll(activity => activity.Value.RecalculateHills(Hills, force, false));
            }
            else
            {
                foreach (KeyValuePair<string, Activity> kvp in CurrentAthlete.Activities)
                {
                    Activity activity = kvp.Value;
                    activity.RecalculateHills(Hills, force, false);
                }
            }

            if (Options.Instance.DebugHills)
                Hill.Dump(Hills, Hill.WAINWRIGHT);

            Logging.Instance.TraceLeave();
        }


        [MemoryPackIgnore]
        public Athlete CurrentAthlete { get { return Athletes.First().Value; } }

        public Athlete FindOrCreateAthlete(string id)
        {
            if (Athletes.ContainsKey(id))
            {
                return Athletes[id];
            }
            //remove dummy athelete
            if (Athletes.ContainsKey(""))
            {
                Athletes.Remove("");
            }
            Athletes.Add(id, new Athlete());
            return Athletes[id];
        }


        private const string DatabaseSerializedName = "Database.bin";
        private const string DatabaseSerializedNameMP = "Database.bin_mp";

        public void SaveToMemoryPackFile()
        {
            PreSerialize();
            string path = Path.Combine(Options.AppDataPath, DatabaseSerializedNameMP);
            SaveToMemoryPack(path);
        }


        public void SaveToBinaryFile(string path)
        {
            Logging.Instance.TraceEntry("Database.SaveToFile");

            PreSerialize();

            if (File.Exists(path))
            {
                string newPath = path + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                File.Move(path, newPath);
            }
#pragma warning disable SYSLIB0011
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                formatter.Serialize(stream, this);
#pragma warning restore SYSLIB0011
            }



            //clean up old backup files (keep 5)
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.DirectoryName != null)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(fileInfo.DirectoryName);
                string searchPattern = fileInfo.Name + "_*";
                var fileInfos = directoryInfo.GetFiles(searchPattern).OrderByDescending(p => p.CreationTime);
                if (fileInfos.Count() > 5)
                {
                    foreach (var file in fileInfos.Skip(5))
                    {
                        file.Delete();
                    }
                }
            }
            Logging.Instance.TraceLeave();

        }

        public void SaveToMemoryPack(string path)
        {
            Logging.Instance.TraceEntry("Database.SaveToMemoryPack");


            var bin = MemoryPackSerializer.Serialize(this); //do the serialization before moving the files around in case we fail


            if (File.Exists(path))
            {
                string newPath = path + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                File.Move(path, newPath);
            }
            using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(bin);
            }

            //clean up old backup files (keep 5)
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.DirectoryName != null)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(fileInfo.DirectoryName);
                string searchPattern = fileInfo.Name + "_*";
                var fileInfos = directoryInfo.GetFiles(searchPattern).OrderByDescending(p => p.CreationTime);
                if (fileInfos.Count() > 5)
                {
                    foreach (var file in fileInfos.Skip(5))
                    {
                        file.Delete();
                    }
                }
            }
            Logging.Instance.TraceLeave();

        }
        public void PostDeserialize()
        {
            foreach (KeyValuePair<string, Athlete> kvp in Athletes)
            {
                kvp.Value.PostDeserialize();
            }
        }

        public void PreSerialize()
        {
            foreach (KeyValuePair<string, Athlete> kvp in Athletes)
            {
                kvp.Value.PreSerialize();
            }
        }

        public static Database LoadFromFile()
        {
            return LoadFromMemoryMapFile();
            //return LoadFromBinaryFile();
        }

        private static Database LoadFromMemoryMapFile()
        {
            string path = Path.Combine(Options.AppDataPath, DatabaseSerializedNameMP);

            if (!File.Exists(path))
            {
                return new Database();
            }
            return LoadFromMemoryMapFile(path);
        }

        public static Database LoadFromBinaryFile(string path)
        {

            Stopwatch deserialize = new Stopwatch();
            deserialize.Start();
            Database retval;
#pragma warning disable SYSLIB0011
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None)) //TODO: allow for loading specific database file, and history of files
            {
                try
                {

                    Object deserialized = formatter.Deserialize(stream);
                    if (deserialized != null && deserialized is Model.Database)
                    {
                        retval = (Model.Database)deserialized;
                    }
                    else
                    {
                        Logging.Instance.Error(string.Format("Derialization failed on {0}", path));
                        throw new Exception(string.Format("Derialization failed on {0}", path));
                    }
#pragma warning restore SYSLIB0011
                    deserialize.Stop();
                    Logging.Instance.Log(string.Format("Derialization took {0}", deserialize.Elapsed));
                    retval.PostDeserialize();
                    return retval;
                }
                catch (Exception e)
                {
                    Logging.Instance.Error(string.Format("Derialization failed with {0}", e));
                    return new Database();
                }
            }
        }

        public static Database LoadFromMemoryMapFile(string path)
        {

            Stopwatch deserialize = new Stopwatch();
            deserialize.Start();
            Database retval;
            try
            {
                byte[] bin = File.ReadAllBytes(path);

                Database? deserialized = MemoryPackSerializer.Deserialize<Database>(bin);
                if (deserialized != null && deserialized is Model.Database)
                {
                    retval = (Model.Database)deserialized;
                }
                else
                {
                    Logging.Instance.Error(string.Format("Derialization failed on {0}", path));
                    throw new Exception(string.Format("Derialization failed on {0}", path));
                }
                deserialize.Stop();
                Logging.Instance.Log(string.Format("Derialization took {0}", deserialize.Elapsed));
                retval.PostDeserialize();
                return retval;
            }
            catch (Exception e)
            {
                Logging.Instance.Error(string.Format("Derialization failed with {0}", e));
                return new Database();
            }
        }

    }
}
