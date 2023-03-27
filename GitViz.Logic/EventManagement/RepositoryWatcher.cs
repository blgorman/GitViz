﻿using System.Reactive.Linq;

namespace GitViz.Logic.EventManagement
{
    public class RepositoryWatcher : IDisposable
    {
        public const int DampeningIntervalInMilliseconds = 1000;

        readonly FileSystemWatcher _watcher;

        public event EventHandler ChangeDetected;

        protected virtual void OnChangeDetected()
        {
            var handler = ChangeDetected;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public RepositoryWatcher(string path, bool isBare)
        {
            string gitFolder;
            if (isBare)
            {
                gitFolder = path;
            }
            else
            {
                gitFolder = Path.Combine(path, @".git");
            }

            _watcher = new FileSystemWatcher(gitFolder)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = true
            };

            //using ReactiveExtensions here
            //first - we setup an observable for all the various FSW events
            var repoChanges = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(handler =>
            {
                _watcher.Changed += handler;
                _watcher.Created += handler;
                _watcher.Deleted += handler;
            },
            handler =>
            {
                _watcher.Changed -= handler;
                _watcher.Created -= handler;
                _watcher.Deleted -= handler;
            });

            //next - we wait until our dampening interval has expired without any new FSW events being raised and
            //at that point trigger the OnChangeDetected method
            repoChanges
                .Throttle(TimeSpan.FromMilliseconds(DampeningIntervalInMilliseconds))
                .Subscribe(h => OnChangeDetected());
        }

        public void Dispose()
        {
            _watcher?.Dispose();
        }

        ~RepositoryWatcher()
        {
            Dispose();
        }
    }
}