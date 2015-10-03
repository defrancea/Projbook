using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NS
{
    public class OneLevelNamespaceClass
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
                    string foo = "42";
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
        }
    }
}

namespace NS2
{
    //namespace NS3
    //{
        namespace NS2.NS3
        {
            class A
            {
            }
        }
    //}
}