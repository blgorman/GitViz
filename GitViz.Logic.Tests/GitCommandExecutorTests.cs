using GitViz.Logic.Tests.TestHelperObjects;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitViz.Logic.Tests
{
    [TestFixture]
    public class GitCommandExecutorTests
    {
        [Test]
        public void ShouldGitInit()
        {
            using (var repo = new TemporaryFolder())
            {
                var executor = new GitCommandExecutor(repo.Path);
                executor.Execute("init");

                var expectedGitFolderPath = Path.Combine(repo.Path, ".git");
                Assert.IsTrue(Directory.Exists(expectedGitFolderPath));
            }
        }

        [Test]
        public void ShouldThrowExceptionForFatalError()
        {
            using (var repo = new TemporaryFolder())
            {
                var executor = new GitCommandExecutor(repo.Path);
                Assert.Throws<ApplicationException>(() => executor.Execute("bad command"));
            }
        }
    }
}
