using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projbook.Documentation.Code
{
    public class SampleClass
    {
        private void Method(string input)
        {
            Console.WriteLine(input);
        }

        private void Method(int input)
        {
            Console.WriteLine(42 + input);
        }
    }
}
