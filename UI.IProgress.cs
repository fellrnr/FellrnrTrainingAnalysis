using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
