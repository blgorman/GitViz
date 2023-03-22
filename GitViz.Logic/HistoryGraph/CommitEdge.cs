using GitViz.Logic.History;
using QuikGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitViz.Logic.HistoryGraph
{
    public class CommitEdge : Edge<Vertex>
    {
        public CommitEdge(Vertex source, Vertex target)
            : base(source, target)
        { 
        }
    }
}
