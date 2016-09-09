﻿using CommandLine;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace SP.Cmd.Deploy
{
    public class SPDeployOptions
    {
        [Option("url", Required = true, HelpText = "SharePoint site url")]
        public string url { get; set; }

        [Option("login", HelpText = "User login name")]
        public string login { get; set; }

        [Option("password", HelpText = "User password")]
        public string password { get; set; }

        [Option("domain", HelpText = "User domain")]
        public string domain { get; set; }

        [Option("spo", HelpText = "Set helper for ")]
        public bool spo { get; set; }

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

            return this.SolutionDescription + "\n"+usage;
        }

        public ICredentials Credentials { get; set; }
        public string SolutionDescription { get; set; }


        public SPDeployOptions()
        {

        }
        public SPDeployOptions(string SolutionDescription)
        {
            this.SolutionDescription = SolutionDescription;
        }
    }
    public static class SharePoint
    {
        public static void CmdExecute(string[] args, string SolutionDescription, Action<SPDeployOptions> DeployFunction, Action<SPDeployOptions> RetractFunction, Action<SPDeployOptions> ExecuteFunction)
        {
            var options = new SPDeployOptions(SolutionDescription);
            if (Parser.Default.ParseArguments(args, options))
            {
                var t = "";
                if (options.url.Length > 0)
                {
                    if ((!String.IsNullOrEmpty(options.login)) && (!String.IsNullOrEmpty(options.password)))
                    {
                        if (options.spo)
                        {
                            var SecurePassword = new SecureString();
                            foreach (char c in options.password.ToCharArray()) SecurePassword.AppendChar(c);
                            options.Credentials = new SharePointOnlineCredentials(options.login, SecurePassword);
                        }
                        else
                        {
                            options.Credentials = new NetworkCredential(options.login, options.password, options.domain);
                        }
                    }

                    if ((options.deploy) && (DeployFunction != null))
                    {
                        DeployFunction(options);
                    }
                    if ((options.retract)&&(RetractFunction != null))
                    {
                        RetractFunction(options);
                    }
                    if ((options.execute) && (ExecuteFunction != null))
                    {
                        ExecuteFunction(options);
                    }
                }
            }
        }
        public static void Session(string url, Action<ClientContext> Code)
        {
            SharePoint.Session(url, null, Code);
        }
        public static void Session(string url, ICredentials Credential, Action<ClientContext> Code)
        {
            using (var clientContext = new ClientContext(url))
            {
                if (Credential != null)
                {
                    clientContext.Credentials = Credential;
                }

                Code(clientContext);
            }
        }
    }
}
