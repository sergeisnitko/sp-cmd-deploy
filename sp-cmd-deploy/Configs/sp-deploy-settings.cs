using SP.Cmd.Deploy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace sp_cmd_deploy
{
    public static class sp_deploy_settings
    {
        private static string SettingsFileName = "configs//sp-cmd-config.xml";

        public static List<System.Reflection.PropertyInfo> GetPropsToSync()
        {
            return typeof(SPDeployOptions).GetProperties().Where(p => p.CustomAttributes.Where(ca => ca.AttributeType == typeof(XmlIgnoreAttribute)).ToList().Count == 0).ToList();
        }

        public static SPDeployOptions GetStartParams(SPDeployOptions options)
        {
            var Result = new SPDeployOptions();

            var SyncProps = GetPropsToSync();
            var SavedOptions = LoadSettings();
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
                        var SaveValueBool = (ConsoleValue.ToLower() == "y") ? true : ((ConsoleValue.ToLower() == "n") ? false : SavedValueBool);

                        SyncProp.SetValue(StartParams, SaveValueBool);
                    }
                    else
                    {
                        if (SyncProp.Name.ToLower() == "password")
                        {
                            Console.Write(": ");

                            var ConsoleValue = GetPassword();
                            var Encrypted = ConsoleValue;
                            if (Encrypted.Trim().Length > 0)
                            {
                                var Decrypted = (new SpSimpleAES()).DecryptString(ConsoleValue);
                                Encrypted = (new SpSimpleAES()).EncryptToString(Decrypted);
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
                            ConsoleValue = ConsoleValue.Length > 0 ? ConsoleValue : (string)Value;

                            SyncProp.SetValue(StartParams, ConsoleValue);
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

        public static void SaveSettings(SPDeployOptions RunSettings)
        {
            var DecryptedPassword = (new SpSimpleAES()).DecryptString(RunSettings.password);
            RunSettings.password = (new SpSimpleAES()).EncryptToString(DecryptedPassword);

            var Serializer = new XmlSerializer(typeof(SPDeployOptions));

            Directory.CreateDirectory(Path.GetDirectoryName(SettingsFileName));

            if (File.Exists(SettingsFileName))
            {
                File.Delete(SettingsFileName);
            }

            using (var fs = new FileStream(SettingsFileName, FileMode.OpenOrCreate))
            {
                Serializer.Serialize(fs, RunSettings);
            }
        }

        public static SPDeployOptions LoadSettings()
        {            
            if (System.IO.File.Exists(SettingsFileName))
            {
                var Serializer = new XmlSerializer(typeof(SPDeployOptions));
                var RunSettings = new SPDeployOptions();
                using (var Reader = new FileStream(SettingsFileName, FileMode.Open))
                {
                    RunSettings = (SPDeployOptions)Serializer.Deserialize(Reader);
                }
                return RunSettings;
            }
            return null;
        }

    }
}
