using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projbook.Extension.Model
{
    public class LineRangeReference
    {
        public int StartLine { get; private set; }

        public int EndLine { get; private set; }

        public LineRangeReference(int startLine, int endLine)
        {
            this.StartLine = startLine;
            this.EndLine = endLine;
        }
    }
}