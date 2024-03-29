using FellrnrTrainingAnalysis.Model;
using FellrnrTrainingAnalysis.Utils;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace FellrnrTrainingAnalysis.UI
{
    public class Goals //TODO make UI.Goals a user control
    {
        public Goals(Database database, DataGridView goalsDataGridView, TextBox goalsTextBox)
        {
            Database = database;
            GoalsDataGridView = goalsDataGridView;
            GoalsTextBox = goalsTextBox;
        }

        private Database Database { get; set; }
        private DataGridView GoalsDataGridView;
        private TextBox GoalsTextBox;

        public void SendGoals()
        {
            if (Database.CurrentAthlete.Activities.Count == 0) //don't bother if we don't have data. It won't end well. 
            {
                return;
            }

            var smtpClient = new SmtpClient(Options.Instance.EmailSmtpHost)
            {
                Port = Options.Instance.EmailSmtpPort,
                Credentials = new NetworkCredential(Options.Instance.EmailAccount, Options.Instance.EmailPassword),
                EnableSsl = true,
            };

            List<Model.Period> periods = Model.Period.DefaultEmailPeriods;
            string body = GetGoalsAsHtml(periods);


            var mailMessage = new MailMessage
            {
                From = new MailAddress(Options.Instance.EmailAccount),
                Subject = "Fellrnr Training Analytics Goal Update",
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(Options.Instance.EmailDestination);
            smtpClient.Send(mailMessage);
        }

        public string GetGoalsAsHtml(List<Model.Period> periods)
        {
            if (Database.CurrentAthlete.Activities.Count == 0) //don't bother if we don't have data. It won't end well. 
            {
                return "";
            }

            List<Goal> goals = GoalFactory.GetGoals();
            //List<int> periods = new List<int>() { 7 };
            StringBuilder sb = new StringBuilder();
            using (Html.Table table = new Html.Table(sb, id: "some-id", tags: "border=\"1\" cellspacing=\"0\" cellpadding=\"2\""))
            {
                table.StartHead();
                using (var thead = table.AddRow())
                {
                    thead.AddCell("Metric");
                    thead.AddCell("Sport");
                    thead.AddCell("⚖");
                    foreach (Model.Period period in periods)
                    {
                        thead.AddCell(string.Format("{0}", period.FullName));
                    }
                }
                table.EndHead();
                table.StartBody();
                foreach (Goal goal in goals)
                {
                    Model.Day latestActivity = Database.CurrentAthlete.Days.Last().Value;
                    Dictionary<Model.Period, float>? rolling = goal.GetGoalUpdate(Database, periods, latestActivity);
                    if (rolling == null)
                        continue;
                    using (var tr = table.AddRow())
                    {
                        tr.AddCell(goal.TargetColumn);
                        tr.AddCell(goal.SportDescription);
                        tr.AddCell(goal.ActivityFieldname);
                        foreach (KeyValuePair<Model.Period, float> kvpResult in rolling)
                        {
                            string cellValue = goal.FormatResult(kvpResult);
                            tr.AddCell(cellValue, tags: "align=\"right\"");
                        }
                    }
                }
                table.EndBody();
            }
            return sb.ToString();
        }

        public void UpdateGoalsGrid()
        {
            if (Database.CurrentAthlete.Activities.Count == 0) //don't bother if we don't have data. It won't end well. 
            {
                return;
            }

            List<Goal> goals = GoalFactory.GetGoals();
            List<Model.Period> periods = Model.Period.DefaultDisplayPeriods;
            //List<int> periods = new List<int>() { 7 };
            GoalsDataGridView.RowHeadersVisible = false;
            GoalsDataGridView.Rows.Clear();
            int extraColumns = 3;
            GoalsDataGridView.ColumnCount = periods.Count + extraColumns;
            GoalsDataGridView.ColumnHeadersVisible = true;
            GoalsDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            int i = 0;
            GoalsDataGridView.Columns[i++].Name = "Metric";
            GoalsDataGridView.Columns[i++].Name = "Sport";
            GoalsDataGridView.Columns[i++].Name = "⚖";
            foreach (Model.Period period in periods)
            {
                GoalsDataGridView.Columns[i++].Name = string.Format("{0}", period.FullName);
            }
            foreach (DataGridViewColumn column in GoalsDataGridView.Columns)
            {
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            foreach (Goal goal in goals)
            {
                string[] row = new string[periods.Count + extraColumns];
                Model.Day latestActivity = Database.CurrentAthlete.Days.Last().Value;
                Dictionary<Model.Period, float>? rolling = goal.GetGoalUpdate(Database, periods, latestActivity);
                if (rolling == null)
                    continue;
                i = 0;
                row[i++] = goal.TargetColumn;
                row[i++] = goal.SportDescription;
                row[i++] = goal.ActivityFieldname;
                foreach (KeyValuePair<Model.Period, float> kvpResult in rolling)
                {
                    row[i++] = goal.FormatResult(kvpResult);
                }
                GoalsDataGridView.Rows.Add(row);
            }
            GoalsDataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

            if (Options.Instance.CopyGoalsToClibboard)
            {
                //save the image to the clipboard for use in other programs
                var totalHeight = GoalsDataGridView.Rows.GetRowsHeight(DataGridViewElementStates.None) + GoalsDataGridView.ColumnHeadersHeight;
                //Add 5 pixels through trial and error. Not happy. 
                var totalWidth = GoalsDataGridView.Columns.GetColumnsWidth(DataGridViewElementStates.Visible) + 5 - GoalsDataGridView.RowHeadersWidth; //remove the width of the hidden row headers 
                using (var bmp = new Bitmap(totalWidth, totalHeight))
                {
                    GoalsDataGridView.DrawToBitmap(bmp, new Rectangle(0, 0, totalWidth, totalHeight));
                    Clipboard.SetImage(bmp);
                }
            }
        }


        public void UpdateGoalsText()
        {
            if (Database.CurrentAthlete.Activities.Count == 0) //don't bother if we don't have data. It won't end well. 
            {
                return;
            }
            List<Model.Period> periods = Model.Period.DefaultDisplayPeriods;
            GoalsTextBox.Text = GetGoalsAsText(periods);
        }
        public string GetGoalsAsText(List<Model.Period> periods)
        {
            StringBuilder stringBuilder = new StringBuilder();

            List<Goal> goals = GoalFactory.GetGoals();
            //List<int> periods = new List<int>() { 7 };
            foreach (Goal goal in goals)
            {
                Model.Day latestActivity = Database.CurrentAthlete.Days.Last().Value;
                Dictionary<Model.Period, float>? rolling = goal.GetGoalUpdate(Database, periods, latestActivity);
                if (rolling == null)
                    continue;
                stringBuilder.Append(string.Format("{0}: ", goal.TargetColumn)); //TODO: should be display name, not column name
                string comma = "";
                stringBuilder.Append(string.Format("{0}-", goal.SportDescription));
                foreach (KeyValuePair<Model.Period, float> kvpResult in rolling)
                {
                    stringBuilder.Append(string.Format("{0}{1}: {2}", comma, kvpResult.Key.FullName, goal.FormatResult(kvpResult)));
                    comma = ", ";
                }
                stringBuilder.AppendLine();
            }
            return stringBuilder.ToString();
        }
    }
}
