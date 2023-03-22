namespace GitViz.Logic.History
{
    public class Vertex
    {
        private readonly object _value;

        public Vertex(object value)
        {
            _value = value;
        }

        public Commit Commit
        {
            get { return _value as Commit; }
        }

        public Reference Reference
        {
            get { return _value as Reference; }
        }

        public bool Orphan { get; set; }
    }
}
