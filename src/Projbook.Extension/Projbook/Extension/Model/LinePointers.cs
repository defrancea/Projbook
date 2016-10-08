using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projbook.Extension.Model
{
    public class LinePointers
    {
        public List<LineReference> LineReferences { get; private set; }

        public List<LineRangeReference> LineRangeReferences { get; private set; }

        public LinePointers()
        {
            this.LineReferences = new List<LineReference>();
            this.LineRangeReferences = new List<LineRangeReference>();
        }

        public void AddPointer(int line)
        {
            this.LineReferences.Add(new LineReference(line));
        }

        public void AddPointer(int start, int end)
        {
            this.LineRangeReferences.Add(new LineRangeReference(start, end));
        }
    }
}