#pragma warning disable 0169
#pragma warning disable 0219
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

        class C<T, U>
        {
            void CMethod<X, Y>(X x, Y y)
            {
            }
        }

        class D
        {
            public int this[string s, int i]
            {
                get
                {
                    return 51;
                }
                set
                {
                    string bar = "bar";
                }
            }

            public event Action Event
            {
                add
                {
                    // Add content
                }
                remove
                {
                    // Remove content
                }
            }
        }

        interface I
        {
            void InterfaceMethod(bool v1, string v2, int v3);
        }

        class WithField
        {
            bool aFieldSomewhere, anotherFieldSomewhere;

            int[] fieldArray;
        }
    }
}
#pragma warning restore 0169
#pragma warning restore 0219