using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using de.schumacher_bw.Strava.Endpoint;
using FellrnrTrainingAnalysis.UI;

namespace FellrnrTrainingAnalysis.Model
{
    [Serializable]
    public class Database
    {

        public Database()
        {
            this.Athletes = new Dictionary<string, Athlete>();
            Athletes.Add("", new Athlete(this)); //TODO: Long term, handle the creation of a new athlete within the app, rather than loading. 
        }

        public Dictionary<string, Athlete> Athletes { get; private set; }

        //reapply the dynamic components, currently dyanamic data streams and goals
        public void Recalculate(bool force)
        {
            foreach (KeyValuePair<string,Athlete> kvp in Athletes)
            {
                Athlete athlete = kvp.Value;
                athlete.Recalculate(force);
            }


            //TODO: the order is unclear here; the goals rely on the data fields, but the calendar node accumulation relies on the goals. 

            List<Goal> goals = Goal.GoalFactory();
            List<Goal.Period> periods = Goal.DefaultStorePeriods;

            foreach (Goal goal in goals)
            {
                goal.UpdateActivityGoals(this, periods, force);
            }
        }

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
            Athletes.Add(id, new Athlete(this));
            return Athletes[id];
        }

        private const string DatabaseSerializedName = "Database.bin";
        public void SaveToFile()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string path = Path.Combine(folder, DatabaseSerializedName);
            SaveToFile(path);
        }
        public void SaveToFile(string path)
        {
            if(File.Exists(path))
            {
                string newPath = path + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                File.Move(path, newPath);
            }
            Stopwatch serialize = new Stopwatch();
            serialize.Start();
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)) //TODO: variable binary filename for database, with backups
            {
#pragma warning disable SYSLIB0011
                formatter.Serialize(stream, this);
#pragma warning restore SYSLIB0011
            }
            serialize.Stop();
            Logging.Instance.Log(string.Format("Serialization took {0}", serialize.Elapsed));
        }

        public static Database LoadFromFile()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string path = Path.Combine(folder, DatabaseSerializedName);
            if (!File.Exists(path))
            {
                return new Database();
            }
            return LoadFromFile(path);
        }
        public static Database LoadFromFile(string path)
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
                    return retval;
                }
                catch (Exception e)
                {
                    Logging.Instance.Log(string.Format("Derialization failed with {0}", e));
                    return new Database();
                }
            }
        }

    }
}
