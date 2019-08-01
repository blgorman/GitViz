using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;

namespace GitViz.Logic
{
    public class GitCommandExecutor
    {
        readonly string _repositoryPath;
        private string _gitPath;

        public GitCommandExecutor(string repositoryPath)
        {
            _repositoryPath = repositoryPath;
            _gitPath = ConfigurationManager.AppSettings["GitPath"] ?? @"C:\Program Files\Git\bin\git.exe";
            if (File.Exists("git.exe"))
            {
                _gitPath = "git.exe";
            }
            
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
            var startInfo = new ProcessStartInfo
            {
                FileName = _gitPath,
                Arguments = command,
                WorkingDirectory = _repositoryPath,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var process = Process.Start(startInfo);
            return process;
        }
    }
}
