using CommandLine;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace SP.Cmd.Deploy
{
    [Serializable]
    public class SPDeployOptions: IDisposable
    {
        [Option("url", HelpText = "SharePoint site url")]

        public string url { get; set; }

        [XmlIgnore]
        [ScriptIgnore]
        [Option("encrypt", HelpText = "Encrypt the string")]
        public string encrypt { get; set; }

        [XmlIgnore]
        [ScriptIgnore]
        [Option("settings", HelpText = "Settings file name")]
        public string Settings { get; set; }

        [Option("login", HelpText = "User login name")]
        public string login { get; set; }

        [Option("password", HelpText = "User password")]
        public string password { get; set; }

        [Option("domain", HelpText = "User domain")]
        public string domain { get; set; }

        [Option("relyingParty", HelpText = "ADFS relying party")]
        public string relyingParty { get; set; }

        [Option("ADFSUrl", HelpText = "ADFS Url")]
        public string ADFSUrl { get; set; }

        [Option("ClientId", HelpText = "SharePoint App ClientId")]
        public string ClientId { get; set; }

        [Option("ClientSecret", HelpText = "SharePoint App ClientSecret")]
        public string ClientSecret { get; set; }

        [Option("ExecuteParams", HelpText = "Enter the keys of functions to execute with a space like a delimiter")]
        public string ExecuteParams { get; set; }

        [XmlIgnore]
        [ScriptIgnore]
        [Option("inlineparams", HelpText = "Inline params for application next time")]
        public bool inlineparams { get; set; }

        [XmlIgnore]
        [ScriptIgnore]
        [Option("spo", HelpText = "Set helper for ")]
        public bool spo { get; set; }

        [XmlIgnore]
        [ScriptIgnore]
        [Option("plain", HelpText = "Use the password as plain")]
        public bool plain { get; set; }


        [Option("deploy", HelpText = "Execute function to deploy the solution (Y[es]/N[o])")]
        [Obsolete("This method is deprecated. Use ExecuteParams instead")]
        [XmlIgnore]
        [ScriptIgnore]
        public bool deploy { get; set; }

        [Option("retract", HelpText = "Execute function to retract the solution (Y[es]/N[o])")]
        [Obsolete("This method is deprecated. Use ExecuteParams instead")]
        [XmlIgnore]
        [ScriptIgnore]
        public bool retract { get; set; }

        [Option("execute", HelpText = "Executes the solution (Y[es]/N[o])")]
        [Obsolete("This method is deprecated. Use ExecuteParams instead")]
        [XmlIgnore]
        [ScriptIgnore]
        public bool execute { get; set; }

        [HelpOption(HelpText = "Command line helper")]
        public string GetUsage()
        {
            var usage = @"To use this solution there are some keys\n" +
                "--url SharePoint site url";

            return this.SolutionDescription + "\n" + usage;
        }

        [XmlIgnore]
        [ScriptIgnore]
        public ICredentials Credentials { get; set; }

        [XmlIgnore]
        [ScriptIgnore]
        public ClientContext Context { get; set; }

        [XmlIgnore]
        [ScriptIgnore]
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
