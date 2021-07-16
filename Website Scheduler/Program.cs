using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Website_Scheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            string EXE_PATH = "";
            string USER_PATH = System.Environment.GetEnvironmentVariable("USERPROFILE")+"\\websitescheduler.exe";
            string WEBSITE = "https://orderhelp.co/index.html";
            int INTERVAL = 15;
            DateTime time = DateTime.Now.Add(new TimeSpan(2, 0, 0));
            string startTime = (time.Hour < 10 ? "0" : "") + time.Hour + ":" + (time.Minute < 10 ? "0" : "") + time.Minute;
            bool forceNew = true;
        createNew:
            forceNew = !forceNew;
            if (args.Count() == 0 ||  forceNew)
            {
                EXE_PATH = System.Reflection.Assembly.GetEntryAssembly().Location;
                if (File.Exists(USER_PATH)) {
                    File.SetAttributes(USER_PATH, FileAttributes.Normal);
                    File.Delete(USER_PATH);
                }
                File.Copy(EXE_PATH, USER_PATH, true);
                File.SetAttributes(USER_PATH, FileAttributes.Normal);
                Console.Write("Exe copied to : "+USER_PATH);

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "schtasks.exe";
                startInfo.Arguments = @"/create /sc minute /mo " + INTERVAL + " /tn WebsiteScheduler /tr " + '"' + USER_PATH + " abcd" + '"' + "' /rl HIGHEST /ST " + startTime;
                startInfo.Verb = "runas";
                process.StartInfo = startInfo;
                process.Start();
            }
            if (args.Count() != 0 || forceNew)
            {
                if (!File.Exists(USER_PATH))
                    goto createNew;
                ExecuteCommand("start "+WEBSITE);
                ExecuteCommand(@"powershell -c ""$wshell = New-Object -ComObject wscript.shell; $wshell.SendKeys('{F11}')");
            }

            //using (var ts = new TaskService())
            //{
            //    // Create a new task definition and assign properties
            //    TaskDefinition td = ts.NewTask();
            //    td.Principal.RunLevel = TaskRunLevel.Highest;
            //    td.Settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew;
            //    td.RegistrationInfo.Description = "Open ";

            //    // Create a trigger that will execute very 2 minutes. 
            //    var trigger = new TimeTrigger();
            //    trigger.Repetition.Interval = TimeSpan.FromMinutes(INTERVAL);
            //    td.Triggers.Add(trigger);

            //    // Create an action that will launch my jobs whenever the trigger fires
            //    td.Actions.Add(new ExecAction(USER_PATH,"fakeargs"));

            //    // Register the task in the root folder
            //    ts.RootFolder.RegisterTaskDefinition(@"Website Scheduler", td);
            //}
        }

        static void ExecuteCommand(string command)
        {
            int exitCode;
            ProcessStartInfo processInfo;
            Process process;

            processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            processInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            // *** Redirect the output ***
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            process = Process.Start(processInfo);
            process.WaitForExit();

            // *** Read the streams ***
            // Warning: This approach can lead to deadlocks, see Edit #2
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            exitCode = process.ExitCode;

            Console.WriteLine("output>>" + (String.IsNullOrEmpty(output) ? "(none)" : output));
            Console.WriteLine("error>>" + (String.IsNullOrEmpty(error) ? "(none)" : error));
            Console.WriteLine("ExitCode: " + exitCode.ToString(), "ExecuteCommand");
            process.Close();
        }
    }
}
