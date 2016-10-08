using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projbook.Extension.Model
{
    public class LineReference
    {
        public int Line { get; private set; }

        public LineReference(int line)
        {
            this.Line = line;
        }
    }
}
