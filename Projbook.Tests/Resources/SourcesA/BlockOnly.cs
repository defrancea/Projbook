using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projbook.Tests.Resources.SourcesA
{
    public class BlockOnly
    {
        void Method()
        {
            // Some content
        }

        public int Property { get; set; }

        event Action Event
        {
            add
            {
                // Something
            }
            remove
            {
                // Something
            }
        }
    }
}
