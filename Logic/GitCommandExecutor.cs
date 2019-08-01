using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;

namespace GitViz.Logic
{
    public class GitCommandExecutor
    {
        readonly string _repositoryPath;

        public GitCommandExecutor(string repositoryPath)
        {
            _repositoryPath = repositoryPath;
        }

        public string Execute(string command)
        {
            var process = CreateProcess(command);
            process.WaitForExit(10000);

            if (process.ExitCode == 0)
                return process.StandardOutput.ReadToEnd();

            var errorText = process.StandardError.ReadToEnd();
            throw new ApplicationException(errorText);
        }

        public StreamReader ExecuteAndGetOutputStream(string command)
        {
            var process = CreateProcess(command);
            return process.StandardOutput;
        }

        Process CreateProcess(string command)
        {
            Process process = null;
            var startInfo = new ProcessStartInfo
            {
                FileName = "git.exe",
                Arguments = command,
                WorkingDirectory = _repositoryPath,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            try
            {
                process = Process.Start(startInfo);
            }
            catch (System.Exception ex)
            {
                var gitPath = ConfigurationManager.AppSettings["GitPath"] ?? @"C:\Program Files\Git\bin\git.exe";
                startInfo = new ProcessStartInfo
                {
                    FileName = gitPath,
                    Arguments = command,
                    WorkingDirectory = _repositoryPath,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                process = Process.Start(startInfo);
            }
             
            return process;
        }
    }
}
