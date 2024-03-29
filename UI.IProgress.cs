namespace FellrnrTrainingAnalysis.UI
{
    public interface IProgress
    {
        string TaskName { get; set; }
        int Maximum { get; set; }
        int Progress { get; set; }

        void ShowMe();

        void HideMe();
    }
}
