using System;
using System.Management.Automation;
using System.Text;
using System.Windows.Forms;
using System.Configuration;

namespace Heath
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void OpenBranch(object sender, EventArgs e)
        {
            string pathToRepo = ConfigurationManager.AppSettings.Get("pathToRepo");
            string bitbucketUrl = ConfigurationManager.AppSettings.Get("bitbucketUrl");
            string workspaceName = ConfigurationManager.AppSettings.Get("workspaceName");
            string repoName = ConfigurationManager.AppSettings.Get("repoName");
            string branchName = ExecuteCommand(new[] { $"cd {pathToRepo}", "git branch --show-current" });
            System.Diagnostics.Process.Start($"{bitbucketUrl}/{workspaceName}/{repoName}/branch/{branchName}");
        }

        private void CreatePR(object sender, EventArgs e)
        {
            string pathToRepo = ConfigurationManager.AppSettings.Get("pathToRepo");
            string bitbucketUrl = ConfigurationManager.AppSettings.Get("bitbucketUrl");
            string workspaceName = ConfigurationManager.AppSettings.Get("workspaceName");
            string repoName = ConfigurationManager.AppSettings.Get("repoName");
            string targetBranch = ConfigurationManager.AppSettings.Get("targetBranch");
            string branchName = ExecuteCommand(new[] { $"cd {pathToRepo}", "git branch --show-current" });
            //https://bitbucket.org/Baydragon/baydragon-website/pull-requests/new?source=user%2Fben%2FfixCPShipping&dest=Baydragon%2Fbaydragon-website%3A%3Amaster&event_source=branch_detail
            //https://bitbucket.org/datacompayroll/christmas/pull-requests/new?source=qa&dest=datacompayroll%2Fchristmas%3A%3Adevelop&event_source=branch_detail

            string prUrl =
                $@"{bitbucketUrl}/{workspaceName}/{repoName}/pull-requests/new?source={branchName}&dest={workspaceName}%2F{repoName}%3A%3A{targetBranch}&event_source=branch_detail";
            System.Diagnostics.Process.Start(prUrl);
        }

        private static string ExecuteCommand(string[] scripts)
        {
            PowerShell _ps = PowerShell.Create();
            string errorMsg = string.Empty;

            foreach (var script in scripts)
            {
                _ps.AddScript(script);
            }

            //Make sure return values are outputted to the stream captured by C#
            _ps.AddCommand("Out-String");

            PSDataCollection<PSObject> outputCollection = new PSDataCollection<PSObject>();
            _ps.Streams.Error.DataAdded += (object sender, DataAddedEventArgs e) =>
            {
                errorMsg = ((PSDataCollection<ErrorRecord>)sender)[e.Index].ToString();
            };

            IAsyncResult result = _ps.BeginInvoke<PSObject, PSObject>(null, outputCollection);

            //Wait for powershell command/script to finish executing
            _ps.EndInvoke(result);

            StringBuilder sb = new StringBuilder();

            foreach (PSObject outputItem in outputCollection)
            {
                sb.AppendLine(outputItem.BaseObject.ToString());
            }

            //Clears the commands we added to the powershell runspace so it's empty the next time we use it
            _ps.Commands.Clear();

            //If an error is encountered, return it
            if (!string.IsNullOrEmpty(errorMsg))
                return errorMsg;

            return sb.ToString().Trim();
        }
    }
}
