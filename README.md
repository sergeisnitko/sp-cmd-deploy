# SharePoint CommandLine Deploy

The solution that helps to execute custom code for SharePoint 2013/2016/Online based on console application (command line arguments) on different stages like deploy, retract or execute

The main idea of the solution is to help to execute custom code in SharePoint in different situations and solutions and to standardize for similar solutions.

The solution has an option to save the settings into the file system for a solution, that helps not to use only command line arguments. 
The solution encrypts the password you pass in, and stores it only in encrypted format

**It is not a compiled application. It is a library!**

*The information from github*

![gif](https://sergeisnitko.github.io/repos/sp-cmd-deploy/sp-cmd-deploy.gif)


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


The *options* object is custom object, that containts parsed params of the application and can be used in you own project
``` c
public class SPDeployOptions
{
    public string url { get; set; } //"SharePoint site url"
    public string login { get; set; } //User login name
    public string password { get; set; } //User password
    public string domain { get; set; } //User domain

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

--deploy // You need to use the key to execute the function mapped to **DeployFunction**
--retract // You need to use the key to execute the function mapped to **RetractFunction**
--execute // You need to use the key to execute the function mapped to **ExecuteFunction** 

//Some extra arguments
--inlineparams //If you sets this param, the solution will ask you to fill every param by inline
```
*The end of information from github*

## Command line in execution
```
fieldsettings.exe --url https://snitko.sharepoint.com/sites/demo --login demo@snitko.onmicrosoft.com --password MyPassword --deploy --spo
```


The saved configuration looks lie this
``` xml
<?xml version="1.0"?>
<SPDeployOptions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <url>http://demo-win.arvosys.com/dev/ey/m2demo3</url>
  <login>sergei.snitko</login>
  <password>037028129147191195214028007188111220179094158225</password>
  <domain>arvo</domain>
  <deploy>true</deploy>
  <retract>true</retract>
  <execute>true</execute>
</SPDeployOptions>
```
end it is fully copies the command line parameters logic


## The terms of 
* Every solution execution saves the parameters you pass into xml file (serialization)
* The file is stored in the folder *configs/sp-cmd-config.xml* of you solution
* You can copy and modify this file from solution to solution
* You can combine command line parameters and parameters from saved file. The solution takes the paramaters from the xml file and overrides them by the parameters from the command line
* The solution executes silent (without asking parameters) if the xml configuration file exists
* You can force the inline passing of parameters by the command line parameter --inline params (like this *fieldsettings.exe --inlineparams*)


