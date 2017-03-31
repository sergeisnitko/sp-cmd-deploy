using CommandLine;
using Microsoft.SharePoint.Client;
using sp_cmd_deploy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace SP.Cmd.Deploy
{

    public static class SharePoint
    {
        public static string EncryptString(string Text)
        {
            return (new SpSimpleAES()).EncryptToString(Text);
        }

        public static void Exec(string[] args, Action<SPDeployOptions> ExecuteFunction)
        {
            CmdExecute(args, "", null, null, ExecuteFunction);
        }

        public static void Exec(string[] args, Action<SPDeployOptions> ExecuteFunction, Action<SPDeployOptions> DeployFunction)
        {
            CmdExecute(args, "", DeployFunction, null, ExecuteFunction);
        }

        public static void Exec(string[] args, Action<SPDeployOptions> ExecuteFunction, Action<SPDeployOptions> DeployFunction, Action<SPDeployOptions> RetractFunction)
        {
            CmdExecute(args, "", DeployFunction, RetractFunction, ExecuteFunction);
        }

        public static void CmdExecute(string[] args, string SolutionDescription, Action<SPDeployOptions> DeployFunction, Action<SPDeployOptions> RetractFunction, Action<SPDeployOptions> ExecuteFunction)
        {
            var options = new SPDeployOptions(SolutionDescription);
            if (Parser.Default.ParseArguments(args, options))
            {
                var t = "";

                options = sp_deploy_settings.GetSettings(options);


                if (options.url.Length > 0)
                {
                    if (options.url.IndexOf(".sharepoint.com") != -1)
                    {
                        options.spo = true;
                    }

                    if ((!String.IsNullOrEmpty(options.login)) && (!String.IsNullOrEmpty(options.password)))
                    {
                        if (!options.plain)
                        {
                            options.password = (new SpSimpleAES()).DecryptString(options.password);
                        }
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

                    SharePoint.Session(options.url, options.Credentials, ctx =>
                    {
                        options.Context = ctx;

                        if ((options.deploy) && (DeployFunction != null))
                        {
                            DeployFunction(options);
                        }
                        if ((options.retract) && (RetractFunction != null))
                        {
                            RetractFunction(options);
                        }
                        if ((options.execute) && (ExecuteFunction != null))
                        {
                            ExecuteFunction(options);
                        }
                    });
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
