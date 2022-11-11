using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Heath
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string gitRepo = @"C:\Users\Ben\source\repos\Heath";
            string bitbucketUrl = @"https://bitbucket.org/Baydragon/baydragon-website/branch/";

            ExecuteCommand($@"cd {gitRepo}");
            string branchName = ExecuteCommand("git branch --show-current");

            System.Diagnostics.Process.Start($"{bitbucketUrl}{branchName}");
        }

        private static string ExecuteCommand(string script)
        {
            PowerShell _ps = PowerShell.Create();
            string errorMsg = string.Empty;

            _ps.AddScript(script);

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
