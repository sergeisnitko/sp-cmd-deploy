using CommandLine;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SP.Cmd.Deploy
{
    public class SPDeployOptions
    {
        [Option("url", Required = true, HelpText = "SharePoint site url")]
        public string url { get; set; }

        [Option("encrypt", HelpText = "Encrypt the string")]
        public string encrypt { get; set; }

        [Option("login", HelpText = "User login name")]
        public string login { get; set; }

        [Option("password", HelpText = "User password")]
        public string password { get; set; }

        [Option("domain", HelpText = "User domain")]
        public string domain { get; set; }

        [Option("spo", HelpText = "Set helper for ")]
        public bool spo { get; set; }

        [Option("plain", HelpText = "Use the the password as plain")]
        public bool plain { get; set; }

        [Option("deploy", HelpText = "Sets to deploy the solution")]
        public bool deploy { get; set; }

        [Option("retract", HelpText = "Sets to retract the solution")]
        public bool retract { get; set; }

        [Option("execute", HelpText = "Executs the solution")]
        public bool execute { get; set; }

        [HelpOption(HelpText = "Command line helper")]
        public string GetUsage()
        {
            var usage = @"To use this solution there are some keys\n" +
                "--url SharePoint site url";

            return this.SolutionDescription + "\n" + usage;
        }

        public ICredentials Credentials { get; set; }

        public ClientContext Context { get; set; }

        public string SolutionDescription { get; set; }


        public SPDeployOptions()
        {

        }
        public SPDeployOptions(string SolutionDescription)
        {
            this.SolutionDescription = SolutionDescription;
        }
    }
}
