# SharePoint CommandLine Deploy

The solution that helps to execute custom code for SharePoint 2013/2016/Online based on console application (command line arguments) on different stages like deploy, retract or execute

The main idea of the solution is to help to execute custom code in SharePoint in different situations and solutions and to standardize for similar solutions

**It is not a compiled application. It is a library!**

![gif](https://raw.githubusercontent.com/sergeisnitko/sergeisnitko.github.io/master/repos/sp-cmd-deploy/sp-cmd-deploy.gif)


# How to install
You can download sources and add a class to you project or download release library and add it like a reference in the project (and additional libraries), but the best solution is to use [nuget sp-cmd-deploy](https://www.nuget.org/packages/sp-cmd-deploy/)
```
Install-Package sp-cmd-deploy
```
# How to use in project
It is prety easy.
After you added a library like a nuget, you just need to add a reference to *SP.Cmd.Deploy* in your *Program.cs* and execute the function *SharePoint.CmdExecute*

You have to pass fome params in *SharePoint.CmdExecute*
* **args** - command line arguments of the application
* **SolutionDescription** - the helper text in command window that shows to user
* **DeployFunction** - the Action thet needs to be executed on **deploy** command. You need to set *null*, if you don't need to  use this option in your solution
* **RetractFunction** - the Action thet needs to be executed on **retract** command. You need to set *null*, if you don't need to  use this option in your solution
* **ExecuteFunction** - the Action thet needs to be executed on **execute** command. You need to set *null*, if you don't need to  use this option in your solution

``` c
using SP.Cmd.Deploy;

namespace spf_testnamespace
{
    class Program
    {
        static void Main(string[] args)
        {
            SharePoint.CmdExecute(args, "SPF test solution",
                options =>
                {
                    Model.Deploy(options);
                },
                options =>
                {
                    Model.Retract(options);
                },
                null
            );
        }
    }
}

```

The *options* object is custom object, that containts parsed params of the application and can be used in you own project
``` c
public class SPDeployOptions
{
    public string url { get; set; } //"SharePoint site url"
    public string login { get; set; } //User login name
    public string password { get; set; } //User password
    public string domain { get; set; } //User domain
    public bool spo { get; set; } // Sets to use SharePoint Online
    public bool deploy { get; set; } //Sets to deploy the solution
    public bool retract { get; set; } //Sets to retract the solution
    public bool execute { get; set; } //Executs the solution
    public ICredentials Credentials { get; set; } //The credentials of the user
    public string SolutionDescription { get; set; } //The solution description
}
```
The *options* are filled by arguments of the command line of the solution 
``` c
--url // the url of SharePoint site you want to execute you solution. It can be SharePoint 2013/SharePoint 2016/SharePoint Online. If you deploy to SharePoint Online, you need to add a key --spo in you command line 
--login // a login account to connect to SharePoint. If you execute your application on SharePoint 2013/SharePoint 2016, you can ignore this option. In this situation, the library would get credentials of current user 
--password // a password of the user, that you set in login param. You need to ignore it, if you ignore the *login* param
--domain //a domain of the user, that you set in login param if it is necessary. You need to ignore it, if you ignore the *login* param
--spo // You need to use the key if you want to execute your solution of SharePoint Online
--deploy // You need to use the key to execute the function mapped to **DeployFunction**
--retract // You need to use the key to execute the function mapped to **RetractFunction**
--execute // You need to use the key to execute the function mapped to **ExecuteFunction** 

```

# The example of usage in project

The full example you can find [here](https://github.com/sergeisnitko/spf-fieldsettings)

## Model.cs
``` c
using Microsoft.SharePoint.Client;
using SP.Cmd.Deploy;
using SPMeta2.BuiltInDefinitions;
using SPMeta2.CSOM.ModelHosts;
using SPMeta2.CSOM.Services;
using SPMeta2.Definitions;
using SPMeta2.Syntax.Default;
using SPMeta2.Syntax.Default.Utils;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace spf_fieldsettings
{
    public static class Model
    {
        public static string Assets = @"SiteAssets";
        public static string SystemPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        public static UserCustomActionDefinition SettingLink()
        {

            return new UserCustomActionDefinition
            {
                Title = "SPFfieldsettings",
                Name = "SPFSfieldsettings",
                ScriptSrc = "~sitecollection/_catalogs/masterpage/spf/settings/spf.fieldsettings.js",
                Location = "ScriptLink",
                Sequence = 100
            };
        }

        public static SiteModelNode DeployModel()
        {
            return SPMeta2Model.NewSiteModel(site =>
            {
                site
                    .AddRootWeb(new RootWebDefinition(), RootWeb =>
                    {
                        RootWeb
                            .AddHostList(BuiltInListDefinitions.Catalogs.MasterPage, list =>
                            {
                                var FolderPath = Path.Combine(SystemPath, Assets);
                                if (Directory.Exists(FolderPath))
                                {
                                    ModuleFileUtils.LoadModuleFilesFromLocalFolder(list, FolderPath);
                                }

                            });

                    })
                    .AddUserCustomAction(SettingLink())
                    ;
            });
        }

        public static void ExecuteModel(this SiteModelNode Model, string url, ICredentials Credential = null)
        {
            SharePoint.Session(url, Credential, ctx =>
            {
                var provisionService = new CSOMProvisionService();
                provisionService.DeployModel(SiteModelHost.FromClientContext(ctx), Model);

            });
        }

        public static void Retract(SPDeployOptions options)
        {
            SharePoint.Session(options.url,options.Credentials, Ctx =>
            {
                var Site = Ctx.Site;
                var CustomActions = Site.UserCustomActions;
                Ctx.Load(CustomActions);
                Ctx.ExecuteQuery();
                var SettingsLinkAction = CustomActions.Where(x => x.Name == SettingLink().Name).FirstOrDefault();
                if (SettingsLinkAction != null)
                {
                    SettingsLinkAction.DeleteObject();
                    Ctx.ExecuteQuery();
                }
            });
        }
        public static void Deploy(SPDeployOptions options)
        {
            DeployModel().ExecuteModel(options.url, options.Credentials);
        }

    }
}

```

## Program.cs
``` c

using SP.Cmd.Deploy;

namespace spf_fieldsettings
{
    class Program
    {
        static void Main(string[] args)
        {
            SharePoint.CmdExecute(args, "SPF Extended settings for fields",
                options =>
                {
                    Model.Deploy(options);
                },
                options =>
                {
                    Model.Retract(options);
                },
                null
            );

            var t = "";
        }
    }
}

```

## Command line in execution
```
fieldsettings.exe --url https://snitko.sharepoint.com/sites/demo --login demo@snitko.onmicrosoft.com --password MyPassword --deploy --spo
```




# Dependencies
* CommandLine - https://github.com/gsscoder/commandline


