using FellrnrTrainingAnalysis.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FellrnrTrainingAnalysis.Utils;

namespace FellrnrTrainingAnalysis.UI
{
    public class Goals
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

            List<Goal.Period> periods = Goal.DefaultEmailPeriods;
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

        public string GetGoalsAsHtml(List<Goal.Period> periods)
        {
            if (Database.CurrentAthlete.Activities.Count == 0) //don't bother if we don't have data. It won't end well. 
            {
                return "";
            }

            List<Goal> goals = Goal.GoalFactory();
            //List<int> periods = new List<int>() { 7 };
            StringBuilder sb = new StringBuilder();
            using (Html.Table table = new Html.Table(sb, id: "some-id", tags: "border=\"1\" cellspacing=\"0\" cellpadding=\"2\""))
            {
                table.StartHead();
                using (var thead = table.AddRow())
                {
                    thead.AddCell("Metric");
                    thead.AddCell("Sport");
                    foreach (Goal.Period period in periods)
                    {
                        thead.AddCell(string.Format("{0}", period.FullName));
                    }
                }
                table.EndHead();
                table.StartBody();
                foreach (Goal goal in goals)
                {
                    Activity latestActivity = Database.CurrentAthlete.Activities.Last().Value;
                    Dictionary<Goal.Period, float>? rolling = goal.GetGoalUpdate(Database, periods, latestActivity);
                    if (rolling == null)
                        continue;
                    using (var tr = table.AddRow())
                    {
                        tr.AddCell(goal.TargetColumn);
                        tr.AddCell(goal.SportDescription);
                        foreach (KeyValuePair<Goal.Period, float> kvpResult in rolling)
                        {
                            string cellValue = string.Format("{0} ({1})", goal.FormatResult(kvpResult.Value), goal.AsPercentTarget(kvpResult.Value, kvpResult.Key.ApproxDays));
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

            List<Goal> goals = Goal.GoalFactory();
            List<Goal.Period> periods = Goal.DefaultDisplayPeriods;
            //List<int> periods = new List<int>() { 7 };
            GoalsDataGridView.RowHeadersVisible = false;
            GoalsDataGridView.Rows.Clear();
            GoalsDataGridView.ColumnCount = periods.Count + 2;
            GoalsDataGridView.ColumnHeadersVisible = true;
            GoalsDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            int i = 0;
            GoalsDataGridView.Columns[i++].Name = "Metric";
            GoalsDataGridView.Columns[i++].Name = "Sport";
            foreach (Goal.Period period in periods)
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
                string[] row = new string[periods.Count + 2];
                Activity latestActivity = Database.CurrentAthlete.Activities.Last().Value;
                Dictionary<Goal.Period, float>? rolling = goal.GetGoalUpdate(Database, periods, latestActivity);
                if (rolling == null)
                    continue;
                i = 0;
                row[i++] = goal.TargetColumn;
                row[i++] = goal.SportDescription;
                foreach (KeyValuePair<Goal.Period, float> kvpResult in rolling)
                {
                    row[i++] = string.Format("{0} ({1})", goal.FormatResult(kvpResult.Value), goal.AsPercentTarget(kvpResult.Value, kvpResult.Key.ApproxDays));
                }
                GoalsDataGridView.Rows.Add(row);
            }
            GoalsDataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

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


        public void UpdateGoalsText()
        {
            if (Database.CurrentAthlete.Activities.Count == 0) //don't bother if we don't have data. It won't end well. 
            {
                return;
            }
            List<Goal.Period> periods = Goal.DefaultDisplayPeriods;
            GoalsTextBox.Text = GetGoalsAsText(periods);
        }
        public string GetGoalsAsText(List<Goal.Period> periods)
        {
            StringBuilder stringBuilder = new StringBuilder();

            List<Goal> goals = Goal.GoalFactory();
            //List<int> periods = new List<int>() { 7 };
            foreach (Goal goal in goals)
            {
                Activity latestActivity = Database.CurrentAthlete.Activities.Last().Value;
                Dictionary<Goal.Period, float>? rolling = goal.GetGoalUpdate(Database, periods, latestActivity);
                if (rolling == null)
                    continue;
                stringBuilder.Append(string.Format("{0}: ", goal.TargetColumn)); //TODO: should be display name, not column name
                string comma = "";
                stringBuilder.Append(string.Format("{0}-", goal.SportDescription));
                foreach (KeyValuePair<Goal.Period, float> kvpResult in rolling)
                {
                    stringBuilder.Append(string.Format("{0}{1}: {2}", comma, kvpResult.Key.FullName, goal.FormatResult(kvpResult.Value)));
                    comma = ", ";
                }
                stringBuilder.AppendLine();
            }
            return stringBuilder.ToString();
        }
    }
}
