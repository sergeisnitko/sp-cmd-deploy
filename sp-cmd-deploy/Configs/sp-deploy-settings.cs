using Newtonsoft.Json;
using SP.Cmd.Deploy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace sp_cmd_deploy
{
    public static class sp_deploy_settings
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        private static string SettingsFileName = Path.Combine(SharePoint.SystemPath,"configs\\sp-cmd-config.xml");
        private static string SettingsJsonFileName = Path.Combine(SharePoint.SystemPath, "configs\\sp-cmd-config.json");

        public static List<System.Reflection.PropertyInfo> GetPropsToSync()
        {
            return typeof(SPDeployOptions).GetProperties().Where(p => p.CustomAttributes.Where(ca => ca.AttributeType == typeof(XmlIgnoreAttribute)).ToList().Count == 0).ToList();
        }

        public static void EchoCurrentParams(SPDeployOptions options)
        {
            if (GetConsoleWindow() != IntPtr.Zero)
            {
                Console.Clear();

                var SyncProps = GetPropsToSync();
                var StartParams = GetStartParams(options);

                foreach (var SyncProp in SyncProps)
                {
                    var HelpTextString = SyncProp.Name;
                    var Value = SyncProp.GetValue(options, null);

                    if ((SyncProp.Name.ToLower() != "password") && (Value != null))
                    {
                        var Attribute = SyncProp.CustomAttributes.Where(a => a.AttributeType == typeof(CommandLine.OptionAttribute)).FirstOrDefault();
                        if (Attribute != null)
                        {
                            var HelpText = Attribute.NamedArguments.Where(a => a.MemberName == "HelpText").FirstOrDefault();

                            if (HelpText != null)
                            {
                                HelpTextString = HelpText.TypedValue.ToString();
                            }

                        }
                        Console.Write(HelpTextString + ": ");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(Value);
                        Console.ResetColor();
                        Console.WriteLine();
                    }
                }
            }
        }


        public static SPDeployOptions GetStartParams(SPDeployOptions options)
        {
            var Result = new SPDeployOptions();

            var SyncProps = GetPropsToSync();
            var SavedOptions = LoadSettings(options);
            var InlineParams = options.inlineparams;
            if (SavedOptions == null)
            {
                InlineParams = true;
                SavedOptions = new SPDeployOptions();
            }

            foreach (var SyncProp in SyncProps)
            {
                var SavedValue = SyncProp.GetValue(SavedOptions, null);
                var Value = SyncProp.GetValue(options, null);

                if (SyncProp.PropertyType == typeof(Boolean))
                {
                    var ResultValueBool = (bool)Value ? Value : SavedValue;
                    SyncProp.SetValue(Result, ResultValueBool);
                }
                else
                { 
                    var ResultValue = !String.IsNullOrEmpty((string)Value) ? Value : SavedValue;
                    SyncProp.SetValue(Result, ResultValue);
                }
            }
            Result.inlineparams = InlineParams;
            return Result;
        }

        public static SPDeployOptions GetSettings(SPDeployOptions options)
        {
            var SyncProps = GetPropsToSync();
            var StartParams = GetStartParams(options);

            if (GetConsoleWindow() != IntPtr.Zero)
            {
                if (StartParams.inlineparams)
                {
                    foreach (var SyncProp in SyncProps)
                    {
                        var HelpTextString = SyncProp.Name;
                        var Value = SyncProp.GetValue(StartParams, null);

                        var Attribute = SyncProp.CustomAttributes.Where(a => a.AttributeType == typeof(CommandLine.OptionAttribute)).FirstOrDefault();
                        if (Attribute != null)
                        {
                            var HelpText = Attribute.NamedArguments.Where(a => a.MemberName == "HelpText").FirstOrDefault();

                            if (HelpText != null)
                            {
                                HelpTextString = HelpText.TypedValue.ToString();
                            }

                        }
                        Console.Write(HelpTextString);

                        if (SyncProp.PropertyType == typeof(Boolean))
                        {
                            var SavedValueBool = Convert.ToBoolean(Value);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(" (" + (SavedValueBool ? "Y" : "N") + ")");
                            Console.ResetColor();
                            Console.Write(": ");

                            var ConsoleValue = Console.ReadLine();
                            if (ConsoleValue == null)
                            {
                                ConsoleValue = "";
                            }
                            var SaveValueBool = (ConsoleValue.ToLower() == "y") ? true : ((ConsoleValue.ToLower() == "n") ? false : SavedValueBool);

                            SyncProp.SetValue(StartParams, SaveValueBool);
                        }
                        else
                        {
                            if (SyncProp.Name.ToLower() == "password")
                            {
                                if (!String.IsNullOrEmpty((string)Value))
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.Write(" (" + Value + ")");
                                    Console.ResetColor();
                                }
                                Console.Write(": ");

                                var ConsoleValue = GetPassword();
                                if (ConsoleValue == null)
                                {
                                    ConsoleValue = "";
                                }
                                var Encrypted = ConsoleValue;
                                if (Encrypted.Trim().Length > 0)
                                {
                                    var Decrypted = (new SpSimpleAES()).DecryptString(ConsoleValue);
                                    Encrypted = (new SpSimpleAES()).EncryptToString(Decrypted);
                                }
                                else
                                {
                                    if (!String.IsNullOrEmpty((string)Value))
                                    {
                                        var Decrypted = (new SpSimpleAES()).DecryptString((string)Value);
                                        Encrypted = (new SpSimpleAES()).EncryptToString(Decrypted);
                                    }
                                }
                                SyncProp.SetValue(StartParams, Encrypted);
                                Console.WriteLine();
                            }
                            else
                            {
                                if (!String.IsNullOrEmpty((string)Value))
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.Write(" (" + Value + ")");
                                    Console.ResetColor();
                                }
                                Console.Write(": ");

                                var ConsoleValue = Console.ReadLine();
                                if (ConsoleValue == null)
                                {
                                    ConsoleValue = "";
                                }
                                ConsoleValue = ConsoleValue.Length > 0 ? ConsoleValue : (string)Value;

                                SyncProp.SetValue(StartParams, ConsoleValue);
                            }
                        }
                    }
                }
            }
            SaveSettings(StartParams);
            return StartParams;
        }
 

        public static String GetPassword()
        {
            var pwd = "";
            while (true)
            {
                ConsoleKeyInfo i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    if (pwd.Length > 0)
                    {
                        pwd = pwd.Substring(0,pwd.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else
                {
                    pwd += i.KeyChar;
                    Console.Write("*");
                }
            }
            return pwd;
        }

        public static string LoadDataFromFile(string SourcePath)
        {
            if (System.IO.File.Exists(SourcePath))
            {
                var StringData = string.Join("\n\r", File.ReadAllLines(SourcePath));
                return StringData;
            }
            return "{}";
        }

        public static void SaveSettings(SPDeployOptions RunSettings)
        {
            SaveSettings(RunSettings, false);
        }
        public static void SaveSettings(SPDeployOptions RunSettings, bool IsXml)
        {
            if (!String.IsNullOrEmpty(RunSettings.Settings))
            {
                SettingsFileName = Path.Combine(SharePoint.SystemPath, "configs\\" + RunSettings.Settings);
            }
            if (!IsXml)
            {
                SettingsFileName = SettingsJsonFileName;
            }
            var DecryptedPassword = (new SpSimpleAES()).DecryptString(RunSettings.password);
            if (!String.IsNullOrEmpty(DecryptedPassword))
            {
                RunSettings.password = (new SpSimpleAES()).EncryptToString(DecryptedPassword);
            }

            var Serializer = new XmlSerializer(typeof(SPDeployOptions));

            Directory.CreateDirectory(Path.GetDirectoryName(SettingsFileName));

            if (File.Exists(SettingsFileName))
            {
                File.Delete(SettingsFileName);
            }

            if (IsXml)
            {
                using (var fs = new FileStream(SettingsFileName, FileMode.OpenOrCreate))
                {
                    Serializer.Serialize(fs, RunSettings);
                }
            }
            else
            {
                var json = JsonConvert.SerializeObject(RunSettings);
                var SW = File.CreateText(SettingsFileName);
                SW.WriteLine(json);
                SW.Close();
            }

        }

        public static SPDeployOptions LoadSettings(SPDeployOptions RunSettings)
        {
            if (!System.IO.File.Exists(SettingsFileName))
            {
                SettingsFileName = SettingsJsonFileName;
            }

            if (!String.IsNullOrEmpty(RunSettings.Settings))
            {
                SettingsFileName = Path.Combine(SharePoint.SystemPath, "configs\\" + RunSettings.Settings);
            }

            if (System.IO.File.Exists(SettingsFileName))
            {

                var ext = Path.GetExtension(SettingsFileName).ToLower();
                if (ext == ".xml")
                {
                    var Serializer = new XmlSerializer(typeof(SPDeployOptions));
                    //var RunSettings = new SPDeployOptions();
                    using (var Reader = new FileStream(SettingsFileName, FileMode.Open))
                    {
                        if (Reader.Length == 0)
                        {
                            return null;                            
                        }
                        RunSettings = (SPDeployOptions)Serializer.Deserialize(Reader);
                    }
                }
                else
                {
                    RunSettings = JsonConvert.DeserializeObject<SPDeployOptions>(LoadDataFromFile(SettingsFileName));
                }
                return RunSettings;
            }
            return null;
        }

    }
}
