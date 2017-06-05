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

        public static void Exec(string[] args, SPFunctions Functions)
        {
            CmdExecute(args, "", Functions);
        }
        
        public static void CmdExecute(string[] args, string SolutionDescription, SPFunctions Functions)
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

                    SharePoint.Session(options, ctx =>
                    {
                        options.Context = ctx;

                        var FunctionsToExecute = options.ExecuteParams.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();

                        FunctionsToExecute.ForEach(FunctionName =>
                        {
                            var Function = Functions.Where(k => k.Key.ToLower() == FunctionName.ToLower()).FirstOrDefault();
                            if (Function.Value != null)
                            {
                                Function.Value(options);
                            }
                        });
                    });
                }
            }
        }

        public static void Session(SPDeployOptions options, Action<ClientContext> Code)
        {

            if (!String.IsNullOrEmpty(options.ADFSUrl))
            {
                OfficeDevPnP.Core.AuthenticationManager am = new OfficeDevPnP.Core.AuthenticationManager();
                using (var clientContext = am.GetADFSUserNameMixedAuthenticatedContext(options.url, options.login, options.password, options.domain, options.ADFSUrl, options.relyingParty, 600))
                {
                    var StartedDate = DateTime.Now;
                    clientContext.ExecutingWebRequest += delegate (object oSender, WebRequestEventArgs webRequestEventArgs)
                    {
                        var CurrentTime = DateTime.Now;
                        var DateDiff = (CurrentTime - StartedDate).TotalMinutes;

                        if (DateDiff > 5)
                        {
                            StartedDate = DateTime.Now;
                            am.RefreshADFSUserNameMixedAuthenticatedContext(options.url, options.login, options.password, options.domain, options.ADFSUrl, options.relyingParty);
                        }                        
                    };

                    Code(clientContext);
                }
            }
            else
            {
                //using (var clientContext = new ClientContext(options.url))
                //{
                var clientContext = new ClientContext(options.url);
                if (options.Credentials != null)
                {
                    clientContext.Credentials = options.Credentials;
                }

                if ((!String.IsNullOrEmpty(options.ClientId)) && (!String.IsNullOrEmpty(options.ClientSecret)))
                {
                    TokenHelper.ClientId = options.ClientId;
                    TokenHelper.ClientSecret = options.ClientSecret;

                    var targetWeb = new Uri(options.url);
                    string targetRealm = TokenHelper.GetRealmFromTargetUrl(targetWeb);
                    var responseToken = TokenHelper.GetAppOnlyAccessToken(TokenHelper.SharePointPrincipal, targetWeb.Authority, targetRealm);
                    clientContext = TokenHelper.GetClientContextWithAccessToken(targetWeb.ToString(), responseToken.AccessToken);
                }

                Code(clientContext);
                clientContext.Dispose();
                //}
            }
        }
    }
}
