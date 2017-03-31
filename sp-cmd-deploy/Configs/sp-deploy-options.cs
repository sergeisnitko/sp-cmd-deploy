using CommandLine;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SP.Cmd.Deploy
{
    [Serializable]
    public class SPDeployOptions: IDisposable
    {
        [XmlIgnore]
        [Option("inlineparams", HelpText = "Inline params for application")]
        public bool inlineparams { get; set; }

        [Option("url", HelpText = "SharePoint site url")]
        public string url { get; set; }

        [XmlIgnore]
        [Option("encrypt", HelpText = "Encrypt the string")]
        public string encrypt { get; set; }

        [Option("login", HelpText = "User login name")]
        public string login { get; set; }

        [Option("password", HelpText = "User password")]
        public string password { get; set; }

        [Option("domain", HelpText = "User domain")]
        public string domain { get; set; }

        [XmlIgnore]
        [Option("spo", HelpText = "Set helper for ")]
        public bool spo { get; set; }

        [XmlIgnore]
        [Option("plain", HelpText = "Use the password as plain")]
        public bool plain { get; set; }

        [Option("deploy", HelpText = "Execute function to deploy the solution (Y[es]/N[o])")]
        public bool deploy { get; set; }

        [Option("retract", HelpText = "Execute function to retract the solution (Y[es]/N[o])")]
        public bool retract { get; set; }

        [Option("execute", HelpText = "Executes the solution (Y[es]/N[o])")]
        public bool execute { get; set; }

        [HelpOption(HelpText = "Command line helper")]
        public string GetUsage()
        {
            var usage = @"To use this solution there are some keys\n" +
                "--url SharePoint site url";

            return this.SolutionDescription + "\n" + usage;
        }

        [XmlIgnore]
        public ICredentials Credentials { get; set; }

        [XmlIgnore]
        public ClientContext Context { get; set; }

        [XmlIgnore]
        public string SolutionDescription { get; set; }


        public SPDeployOptions()
        {

        }
        public SPDeployOptions(string SolutionDescription)
        {
            this.SolutionDescription = SolutionDescription;
        }
        public void Dispose()
        {

        }
    }
}
