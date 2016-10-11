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

    /// <summary>
    /// Some comment that needs to { be } escaped.
    /// </summary>
    public class ConflictingCommentsClass{
        public void Method([Test("Value that needs to { be } escaped")] string p){
            Console.WriteLine("Some code");
        }
    }

    /// <summary>
    /// Some comment that needs to { be } escaped.
    /// </summary>
    public class ConflictingCommentsClass2
    {
        public void Method([Test("Value that needs to { be } escaped")] string p)
        {
            Console.WriteLine("Some code");
        }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class TestAttribute : Attribute
    {
        public TestAttribute(string value)
        {
        }
    }
}
