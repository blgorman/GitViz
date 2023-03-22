using GitViz.Logic.EventManagement;
using GitViz.Logic.History;
using GitViz.Logic.HistoryGraph;
using QuikGraph;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GitViz.Logic
{
    public class Graphing : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public const string GraphPropertyEvent = "Graph";
        public const string DisplayInfoPropertyEvent = "DisplayInfo";

        private readonly LogParser _parser;
        private string _repoPath;
        public string GitPath { get; set; } = "git.exe";
        private GitCommandExecutor _commandExecutor;
        private LogRetriever _logRetriever;
        private RepositoryWatcher? _repositoryWatcher;
       
        public int CommitListSize { get; set; } = 20;

        public string ActiveRefName { get; private set; } = string.Empty;
        public bool IsNewRepository { get; set; } = false;
        public string RepositoryPath { 
            get { return _repoPath; }
            set {
                _repoPath = value.TrimEnd();
                if (Validation.IsValidGitRepository(_repoPath))
                {
                    RefreshGraph();
                    IsNewRepository = true;

                    _repositoryWatcher = new RepositoryWatcher(_repoPath, Validation.IsBareGitRepository(_repoPath));
                    _repositoryWatcher.ChangeDetected += (sender, args) => RefreshGraph();
                    OnPropertyChanged(DisplayInfoPropertyEvent);
                }
                else
                {
                    ResetCommitData(false);
                    OnPropertyChanged(GraphPropertyEvent);
                    if (_repositoryWatcher != null)
                    {
                        _repositoryWatcher.Dispose();
                        _repositoryWatcher = null;
                    }
                }
            } 
        }

        public string DisplayInfo
        {
            get
            {
                var pathRef = "GitViz (Beta) ";
                if (!string.IsNullOrWhiteSpace(_repoPath))
                { 
                    pathRef = $"{pathRef} - {Path.GetFileName(_repoPath)}";
                }
                return pathRef;
            }
        }

        public CommitGraph Graph
        {
            get { return CommitGraph; }
        }

        public List<Commit> Commits { get; private set; } = new List<Commit>();
        public List<string> ReachableCommitHashes { get; private set; } = new List<string>();
        public List<string> UnreachableCommitHashes { get; private set; } = new List<string>();
        public List<Commit> UnreachableCommits { get; private set; } = new List<Commit>();

        public List<Vertex> CommitVertices { get; private set; } = new List<Vertex>();

        public CommitGraph CommitGraph { get; private set; } = new CommitGraph();

        public Graphing()
        {
            _parser = new LogParser();
        }

        public Graphing(string repoPath, string gitExecutablePath, int commitListSize = 20)
        {
            _parser = new LogParser();
            GitPath = gitExecutablePath;
            CommitListSize = commitListSize;
            RepositoryPath = repoPath;
        }

        private void ResetCommitData(bool newRepo)
        {
            var newActiveRef = string.Empty;
            if (_logRetriever is not null)
            { 
                newActiveRef = _logRetriever.GetActiveReferenceName();
            }
            ActiveRefName = newActiveRef;
            Commits = new List<Commit>();
            UnreachableCommits = new List<Commit>();
            ReachableCommitHashes = new List<string>();
            UnreachableCommitHashes = new List<string>();
            CommitGraph = new CommitGraph();
            CommitVertices = new List<Vertex>();
            IsNewRepository = newRepo;
        }

        private void RefreshGraph(bool resetData = true)
        {
            _commandExecutor = new GitCommandExecutor(RepositoryPath, GitPath);
            _logRetriever = new LogRetriever(_commandExecutor, _parser);
            ResetCommitData(true);
            RetrieveCommitHistory();
            CreateGraph();
        }

        private void RetrieveCommitHistory(bool resetData = true) 
        {
            if (resetData)
            {
                ResetCommitData(true);
            }
            
            SetCommits();
            SetReachableCommitHashes();
            SetUnreachableCommitHashes();
            SetUnreachableCommits();
            SetCommitVertices();
        }

        private void SetCommits() 
        {
            Commits = _logRetriever.GetRecentCommits(CommitListSize).ToList();
        }

        private void SetReachableCommitHashes()
        { 
            if (Commits == null || !Commits.Any()) 
            {
                SetCommits();
            }
            ReachableCommitHashes = Commits?.Select(c => c.Hash).ToList() ?? new List<string>();
        }

        private void SetUnreachableCommitHashes()
        {
            UnreachableCommitHashes = _logRetriever.GetRecentUnreachableCommitHashes(CommitListSize).ToList();
        }

        private void SetUnreachableCommits()
        {
            if (UnreachableCommitHashes == null || !UnreachableCommitHashes.Any())
            {
                SetUnreachableCommitHashes();
            }

            if (UnreachableCommitHashes is null)
            {
                UnreachableCommits = new List<Commit>();
                return;
            }

            UnreachableCommits = _logRetriever
                                .GetSpecificCommits(UnreachableCommitHashes)
                                .Where(c => !ReachableCommitHashes.Contains(c.Hash))
                                .ToList();
        }

        private void SetCommitVertices()
        {
            CommitVertices = Commits.Select(c => new Vertex(c))
                            .Union(UnreachableCommits.Select(
                                    c => new Vertex(c) 
                                        { Orphan = true }))
                            .ToList();
        }

        public void CreateGraph()
        {
            // Create the head vertex
            var headVertex = new Vertex(
                                new Reference
                                {
                                        Name = Reference.HEAD,
                                });

            //add commit vertexes to the graph
            foreach (var cv in CommitVertices)
            {
                CommitGraph.AddVertex(cv);
                
                var commitish = cv.Commit;
                if (commitish.Refs is null || !commitish.Refs.Any()) { continue; }

                var isHeadHere = false;
                var isHeadSet = false;

                foreach (var refName in commitish.Refs)
                {
                    if (refName == Reference.HEAD)
                    {
                        isHeadHere = true;
                        CommitGraph.AddVertex(headVertex);
                        continue;
                    }

                    var refVertex = new Vertex(new Reference
                    {
                        Name = refName,
                        IsActive = refName == ActiveRefName
                    });

                    CommitGraph.AddVertex(refVertex);
                    CommitGraph.AddEdge(new CommitEdge(refVertex, cv));

                    var referenceish = refVertex.Reference;
                    if (!referenceish.IsActive) { continue; }
                        
                    isHeadSet = true;
                    CommitGraph.AddEdge(new CommitEdge(headVertex, refVertex));
                }

                if (isHeadHere && !isHeadSet)
                {
                    CommitGraph.AddEdge(new CommitEdge(headVertex, cv));
                }
            }

            //add edges
            foreach (var commitVertex in CommitVertices.Where(c => c.Commit.ParentHashes != null))
            {
                foreach (var parentHash in commitVertex.Commit.ParentHashes)
                {
                    var parentVertex = CommitVertices.SingleOrDefault(c => c.Commit.Hash == parentHash);
                    if (parentVertex != null) CommitGraph.AddEdge(new CommitEdge(commitVertex, parentVertex));
                }
            }

            OnPropertyChanged(GraphPropertyEvent);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
