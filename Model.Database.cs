using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using FellrnrTrainingAnalysis.Utils;
using System.Text.Json;
using MemoryPack;
using de.schumacher_bw.Strava.Endpoint;
using System;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]
    public partial class Database
    {
        static string AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        static string AppDataSubFolder = "FellrnrTrainingData";
        static string AppDataPath = Path.Combine(AppDataFolder, AppDataSubFolder);

        public Database()
        {
            this.Athletes = new Dictionary<string, Athlete>();
            Athletes.Add("", new Athlete()); //TODO: Long term, handle the creation of a new athlete within the app, rather than loading. 
        }

        [MemoryPackInclude]
        public Dictionary<string, Athlete> Athletes { get; private set; }

        [MemoryPackInclude]
        public List<Hill>? Hills { get; set; } = null;


        static bool ForceHillsOnce = false; //useful for debugging hill problems



        //reapply the dynamic components, currently dyanamic data streams and goals
        public void MasterRecalculate(bool force)
        {
            Logging.Instance.StartResetTimer("MasterRecalculate");
            bool forceHills = (ForceHillsOnce || force);
            ForceHillsOnce = false;


            if (Hills == null || Hills.Count == 0 || forceHills)
            {
                Hills = Hill.Reload();
            }


            foreach (KeyValuePair<string,Athlete> kvp in Athletes)
            {
                Athlete athlete = kvp.Value;
                athlete.Recalculate(force);

                Logging.Instance.StartResetTimer();
            }


            //forceHills will reload the hills, resetting the climbed. Calling RecalculateHills will also clear the climbed field
            //if (forceHills)
            //{
            //    foreach (Hill hill in Hills)
            //    {
            //        hill.Climbed = new List<Activity>();
            //    }
            //}
            foreach (KeyValuePair<string, Activity> kvp in CurrentAthlete.Activities)
            {
                Activity activity = kvp.Value;
                activity.RecalculateHills(Hills, forceHills, false);
            }

            if (forceHills && Options.Instance.LogLevel == Options.Level.Debug)
                Hill.Dump(Hills, Hill.WAINWRIGHT);


            //TODO: the order is unclear here; the goals rely on the data fields, but the calendar node accumulation relies on the goals. 
            List<Goal> goals = GoalFactory.GetGoals();
            List<Model.Period> periods = Model.Period.DefaultStorePeriods;

            foreach (Goal goal in goals)
            {
                goal.UpdateActivityGoals(this, periods, force);
            }


            Logging.Instance.Log(string.Format("MasterRecalculate took {0}", Logging.Instance.GetAndResetTime("MasterRecalculate")));
        }

        [MemoryPackIgnore]
        public Athlete CurrentAthlete { get { return Athletes.First().Value; } }

        public Athlete FindOrCreateAthlete(string id)
        {
            if(Athletes.ContainsKey(id))
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
        public void SaveToFile()
        {
            SaveToBinaryFile();
            SaveToMemoryPackFile();
        }

        private void SaveToBinaryFile()
        {
            string path = Path.Combine(AppDataPath, DatabaseSerializedName);
            SaveToBinaryFile(path);
        }
        private void SaveToMemoryPackFile()
        {
            string path = Path.Combine(AppDataPath, DatabaseSerializedNameMP);
            SaveToMemoryPack(path);
        }

        public void SaveToFile(string path)
        {
            //SaveToBinaryFile(path);
            SaveToMemoryPack(path);
        }

        private void SaveToBinaryFile(string path)
        {
            Logging.Instance.Enter("Database.SaveToFile");

            if (File.Exists(path))
            {
                string newPath = path + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                File.Move(path, newPath);
            }
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)) 
            {
#pragma warning disable SYSLIB0011
                formatter.Serialize(stream, this);
#pragma warning restore SYSLIB0011
            }



            //clean up old backup files (keep 5)
            FileInfo fileInfo= new FileInfo(path);
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
            Logging.Instance.Leave();

        }

        private void SaveToMemoryPack(string path)
        {
            Logging.Instance.Enter("Database.SaveToMemoryPack");

            if (File.Exists(path))
            {
                string newPath = path + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                File.Move(path, newPath);
            }

            using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var bin = MemoryPackSerializer.Serialize(this);
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
            Logging.Instance.Leave();

        }
        public void PostDeserialize()
        {
            foreach (KeyValuePair<string, Athlete> kvp in Athletes)
            {
                kvp.Value.PostDeserialize();
            }
        }

        public static Database LoadFromFile()
        {
            if (!Directory.Exists(AppDataPath))
            {
                Directory.CreateDirectory(AppDataPath);
            }
            return LoadFromMemoryMapFile();
            //return LoadFromBinaryFile();
        }

        private static Database LoadFromBinaryFile()
        {
            string path = Path.Combine(AppDataPath, DatabaseSerializedName);

            if (!File.Exists(path))
            {
                return new Database();
            }
            return LoadFromBinaryFile(path);
        }

        private static Database LoadFromMemoryMapFile()
        {
            string path = Path.Combine(AppDataPath, DatabaseSerializedNameMP);

            if (!File.Exists(path))
            {
                return new Database();
            }
            return LoadFromMemoryMapFile(path);
        }

        public static Database LoadFromFile(string path)
        {
            return LoadFromBinaryFile(path);
        }
        private static Database LoadFromBinaryFile(string path)
        {

            Stopwatch deserialize = new Stopwatch();
            deserialize.Start();
            Database retval;
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None)) //TODO: allow for loading specific database file, and history of files
            {
#pragma warning disable SYSLIB0011
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

        private static Database LoadFromMemoryMapFile(string path)
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
