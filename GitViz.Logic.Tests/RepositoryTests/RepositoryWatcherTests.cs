using GitViz.Logic.EventManagement;
using GitViz.Logic.Tests.TestHelperObjects;
using NUnit.Framework;
using System;
using System.Threading;

namespace GitViz.Logic.Tests.RepositoryTests
{
    [TestFixture]
    public class RepositoryWatcherTests
    {
        [Test]
        public void ShouldDetectChangeWhenCreatingNewBranchButNotSwitching()
        {
            Test(
                repo => { },
                repo => repo.RunCommand("branch foo"));
        }

        [Test]
        public void ShouldDetectChangeWhenCreatingNewTag()
        {
            Test(
                repo => { },
                repo => repo.RunCommand("tag foo"));
        }

        [Test]
        public void ShouldDetectChangeWhenCheckingOutADifferentHead()
        {
            Test(
                repo => repo.RunCommand("checkout -b foo"),
                repo => repo.RunCommand("checkout main"));
        }

        internal void Test(Action<TemporaryRepository> preSteps, Action<TemporaryRepository> triggerSteps)
        {
            using (var tempFolder = new TemporaryFolder())
            {
                var tempRepository = new TemporaryRepository(tempFolder);

                tempRepository.RunCommand("init");
                tempRepository.TouchFileAndCommit();

                preSteps(tempRepository);

                // Let everything stablize
                Thread.Sleep(RepositoryWatcher.DampeningIntervalInMilliseconds * 2);

                var triggered = false;
                var watcher = new RepositoryWatcher(tempFolder.Path, false);
                watcher.ChangeDetected += (sender, args) => { triggered = true; };

                triggerSteps(tempRepository);

                Thread.Sleep(RepositoryWatcher.DampeningIntervalInMilliseconds * 2);

                Assert.IsTrue(triggered);
            }
        }
    }
}
