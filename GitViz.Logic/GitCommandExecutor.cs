using System;
using System.Diagnostics;
using System.IO;

namespace GitViz.Logic
{
    public class GitCommandExecutor
    {
        private readonly string _repositoryPath;
        private readonly string _gitExePath = "git.exe"; //works when GIT is on the user PATH
        /// <summary>
        /// Set up the Executor
        /// </summary>
        /// <param name="repositoryPath">Path to the repository</param>
        public GitCommandExecutor(string repositoryPath)
        {
            _repositoryPath = repositoryPath;
        }

        /// <summary>
        /// Set up the Executor
        /// </summary>
        /// <param name="repositoryPath">Path to the repository</param>
        /// <param name="gitPath">Fully qualified path to `git.exe`</param>
        public GitCommandExecutor(string repositoryPath, string gitPath)
        {
            _repositoryPath = repositoryPath;
            //Note: if user path is not set, gitPath should be the fully qualified
            //      path to `git.exe`, such as "C:\Program Files\Git\bin\git.exe"
            _gitExePath = gitPath;
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

        private Process CreateProcess(string command)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _gitExePath,
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