using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JpegTest
{
    static class JPHide
    {

        public static void Hide(string imagePath, string passWord)
        {
            Collection<PSObject> results;

            string fullPath = System.IO.Path.GetFullPath(imagePath);
            string pathToOutFile = ".\\outImages\\" + imagePath;
            pathToOutFile = System.IO.Path.GetFullPath(pathToOutFile);
            System.IO.File.WriteAllLines("test.txt", new string[] { passWord });
            string hiddenFile = System.IO.Path.GetFullPath(@".\test.txt");
            RunspaceConfiguration runspaceConfiguration = RunspaceConfiguration.Create();

            Runspace runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration);
            runspace.Open();

            RunspaceInvoke scriptInvoker = new RunspaceInvoke(runspace);

            Pipeline pipeline = runspace.CreatePipeline();

            //Here's how you add a new script with arguments
            Command myCommand = new Command(@".\script.bat");

            CommandParameter pathParameter = new CommandParameter(fullPath);
            CommandParameter passWordParameter = new CommandParameter(passWord);
            CommandParameter pathToOutputFileParameter = new CommandParameter(pathToOutFile);
            CommandParameter pathToHiddenFileParameter = new CommandParameter(hiddenFile);
            myCommand.Parameters.Add(pathParameter);
            myCommand.Parameters.Add(passWordParameter);
            myCommand.Parameters.Add(pathToOutputFileParameter);
            myCommand.Parameters.Add(pathToHiddenFileParameter);

            pipeline.Commands.Add(myCommand);

            // Execute PowerShell script
            //results = pipeline.Invoke();

            var info = new ProcessStartInfo("cmd.exe");
            info.UseShellExecute = false;
            info.RedirectStandardInput = true;

            var process = Process.Start(info);
            Thread.Sleep(1000);
            process.StandardInput.WriteLine("jphide \"" + pathParameter.Name + "\" \""  + pathToOutputFileParameter.Name + 
                "\" \"" + pathToHiddenFileParameter.Name + "\"");
            process.StandardInput.WriteLine("echo " + passWord);
        }

    }
}
