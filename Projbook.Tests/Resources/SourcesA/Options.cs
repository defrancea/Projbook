using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projbook.Tests.Resources.SourcesA
{
    public class Options
    {
        void Method()
        {
            // Some content
        }

        void EmptyMethod(string i){}

        public int Property { get; set; }

        event Action Event
        {
            add
            {
                // Something
            }
            remove
            {
            }
        }
    }
}
