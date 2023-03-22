using GitViz.Logic.History;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace GitViz
{
    public class VertexTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CommitTemplate { get; set; }
        public DataTemplate ReferenceTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var vertex = item as Vertex;
            if (vertex == null) return base.SelectTemplate(item, container);

            if (vertex.Commit != null) return CommitTemplate;
            if (vertex.Reference != null) return ReferenceTemplate;

            throw new NotSupportedException();
        }
    }
}
