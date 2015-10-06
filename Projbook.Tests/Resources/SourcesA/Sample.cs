using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NS
{
    public class OneClassSomewhere
    {
        private class SubClass
        {
            public int WhateverProperty
            {
                get
                {
                    return 42;
                }
                set
                {
                    string foo = "foo";
                }
            }
        }

        protected bool Foo(string foo)
        {
            return true;
        }

        protected bool Foo(string foo, int bar)
        {
            return false;
        }
    }
    namespace NS2.NS3
    {
        class A
        {
            // In NS.NS2.NS3
        }
    }
}

namespace NS2
{
    namespace NS2.NS3
    {
        class A
        {
            // In NS2.NS2.NS3
        }

        class B
        {
            B(int foo)
            {
            }

            ~B()
            {
            }
        }
    }
}