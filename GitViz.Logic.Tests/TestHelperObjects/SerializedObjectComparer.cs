using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitViz.Logic.Tests.TestHelperObjects
{
    public class SerializedObjectComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            return string.CompareOrdinal(x.ToJson(), y.ToJson());
        }
    }
}
