using NUnit.Framework;
using Projbook.Core.Exception;
using Projbook.Core.Snippet.CSharp;
using System;
using System.IO;

namespace Projbook.Tests.Core.Snippet
{
    /// <summary>
    /// Tests <see cref="CSharpSnippetExtractor"/>.
    /// </summary>
    [TestFixture]
    public class CSharpSnippetExtractorTests : AbstractSnippetTests
    {
        /// <summary>
        /// Tests with invalid input.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void WrongInitSourceDefaultDirectories()
        {
            new CSharpSnippetExtractor("Foo.cs");
        }
        /// <summary>
        /// Tests with invalid input.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void WrongInitSourceEmptyDirectories()
        {
            new CSharpSnippetExtractor("Foo.cs", new DirectoryInfo[0]);
        }

        /// <summary>
        /// Tests with invalid input.
        /// </summary>
        [Test]
        [ExpectedException(typeof(SnippetExtractionException))]
        public void WrongInitEmpty()
        {
            new CSharpSnippetExtractor(null, new DirectoryInfo[] { new DirectoryInfo("Foo") });
            new CSharpSnippetExtractor(string.Empty, new DirectoryInfo[] { new DirectoryInfo("Foo") });
            new CSharpSnippetExtractor("   ", new DirectoryInfo[] { new DirectoryInfo("Foo") });
        }
        
        /// <summary>
        /// Tests extract snippet.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="expectedFile">The expected file.</param>
        [Test]

        // Whole file
        [TestCase("AnyClass.cs", "AnyClass.txt")]

        // Simple matching
        [TestCase("Sample.cs NS", "NS.txt")]

        // Match class
        [TestCase("Sample.cs NS.OneClassSomewhere", "OneClassSomewhere.txt")]
        [TestCase("Sample.cs OneClassSomewhere", "OneClassSomewhere.txt")]

        // Match subclass
        [TestCase("Sample.cs NS.OneClassSomewhere.SubClass", "SubClass.txt")]
        [TestCase("Sample.cs OneClassSomewhere.SubClass", "SubClass.txt")]
        [TestCase("Sample.cs SubClass", "SubClass.txt")]

        // Match property
        [TestCase("Sample.cs NS.OneClassSomewhere.SubClass.WhateverProperty", "WhateverProperty.txt")]
        [TestCase("Sample.cs OneClassSomewhere.SubClass.WhateverProperty", "WhateverProperty.txt")]
        [TestCase("Sample.cs SubClass.WhateverProperty", "WhateverProperty.txt")]
        [TestCase("Sample.cs WhateverProperty", "WhateverProperty.txt")]

        // Match property getter
        [TestCase("Sample.cs NS.OneClassSomewhere.SubClass.WhateverProperty.get", "WhateverPropertyget.txt")]
        [TestCase("Sample.cs OneClassSomewhere.SubClass.WhateverProperty.get", "WhateverPropertyget.txt")]
        [TestCase("Sample.cs SubClass.WhateverProperty.get", "WhateverPropertyget.txt")]
        [TestCase("Sample.cs WhateverProperty.get", "WhateverPropertyget.txt")]

        // Match property setter
        [TestCase("Sample.cs NS.OneClassSomewhere.SubClass.WhateverProperty.set", "WhateverPropertyset.txt")]
        [TestCase("Sample.cs OneClassSomewhere.SubClass.WhateverProperty.set", "WhateverPropertyset.txt")]
        [TestCase("Sample.cs SubClass.WhateverProperty.set", "WhateverPropertyset.txt")]
        [TestCase("Sample.cs WhateverProperty.set", "WhateverPropertyset.txt")]

        // Match indexer
        [TestCase("Sample.cs NS2.NS2.NS3.D.[string,int]", "Indexer.txt")]
        [TestCase("Sample.cs NS2.NS3.D.[string,int]", "Indexer.txt")]
        [TestCase("Sample.cs D.[string,int]", "Indexer.txt")]
        [TestCase("Sample.cs [string,int]", "Indexer.txt")]

        // Match indexer getter
        [TestCase("Sample.cs NS2.NS2.NS3.D.[string,int].get", "Indexerget.txt")]
        [TestCase("Sample.cs NS2.NS3.D.[string,int].get", "Indexerget.txt")]
        [TestCase("Sample.cs D.[string,int].get", "Indexerget.txt")]
        [TestCase("Sample.cs [string,int].get", "Indexerget.txt")]

        // Match all getter and setter
        [TestCase("Sample.cs set", "set.txt")]
        [TestCase("Sample.cs get", "get.txt")]

        // Match event
        [TestCase("Sample.cs NS2.NS2.NS3.D.Event", "Event.txt")]
        [TestCase("Sample.cs NS2.NS3.D.Event", "Event.txt")]
        [TestCase("Sample.cs D.Event", "Event.txt")]
        [TestCase("Sample.cs Event", "Event.txt")]

        // Match event adder
        [TestCase("Sample.cs NS2.NS2.NS3.D.Event.add", "Eventadd.txt")]
        [TestCase("Sample.cs NS2.NS3.D.Event.add", "Eventadd.txt")]
        [TestCase("Sample.cs D.Event.add", "Eventadd.txt")]
        [TestCase("Sample.cs Event.add", "Eventadd.txt")]
        [TestCase("Sample.cs add", "Eventadd.txt")]

        // Match event remover
        [TestCase("Sample.cs NS2.NS2.NS3.D.Event.remove", "Eventremove.txt")]
        [TestCase("Sample.cs NS2.NS3.D.Event.remove", "Eventremove.txt")]
        [TestCase("Sample.cs D.Event.remove", "Eventremove.txt")]
        [TestCase("Sample.cs Event.remove", "Eventremove.txt")]
        [TestCase("Sample.cs remove", "Eventremove.txt")]

        // Match indexer setter
        [TestCase("Sample.cs NS2.NS2.NS3.D.[string,int].set", "Indexerset.txt")]
        [TestCase("Sample.cs NS2.NS3.D.[string,int].set", "Indexerset.txt")]
        [TestCase("Sample.cs D.[string,int].set", "Indexerset.txt")]
        [TestCase("Sample.cs [string,int].set", "Indexerset.txt")]

        // Match method with overloads
        [TestCase("Sample.cs NS.OneClassSomewhere.Foo", "Foo.txt")]
        [TestCase("Sample.cs OneClassSomewhere.Foo", "Foo.txt")]
        [TestCase("Sample.cs Foo", "Foo.txt")]

        // Match method signature
        [TestCase("Sample.cs NS.OneClassSomewhere.Foo(string)", "FooString.txt")]
        [TestCase("Sample.cs OneClassSomewhere.Foo(string)", "FooString.txt")]
        [TestCase("Sample.cs Foo(string)", "FooString.txt")]
        [TestCase("Sample.cs (string)", "FooString.txt")]
        [TestCase("Sample.cs NS.OneClassSomewhere.Foo(string, int)", "FooStringInt.txt")]
        [TestCase("Sample.cs OneClassSomewhere.Foo(string, int)", "FooStringInt.txt")]
        [TestCase("Sample.cs Foo(string, int)", "FooStringInt.txt")]
        [TestCase("Sample.cs (string, int)", "FooStringInt.txt")]

        // Match sub namespace with overlapping
        [TestCase("Sample.cs NS.NS2.NS3", "NSNS2NS3.txt")]
        [TestCase("Sample.cs NS.NS2.NS3.A", "NSNS2NS3A.txt")]
        [TestCase("Sample.cs NS2", "NS2.txt")]
        [TestCase("Sample.cs NS2.NS2.NS3", "NS2NS2NS3.txt")]
        [TestCase("Sample.cs NS2.NS2.NS3.A", "NS2NS2NS3A.txt")]

        // Match constructor
        [TestCase("Sample.cs NS2.NS2.NS3.B.<Constructor>", "Constructor.txt")]
        [TestCase("Sample.cs NS2.NS3.B.<Constructor>", "Constructor.txt")]
        [TestCase("Sample.cs B.<Constructor>", "Constructor.txt")]
        [TestCase("Sample.cs <Constructor>", "Constructor.txt")]

        // Match destructor
        [TestCase("Sample.cs NS2.NS2.NS3.B.<Destructor>", "Destructor.txt")]
        [TestCase("Sample.cs NS2.NS3.B.<Destructor>", "Destructor.txt")]
        [TestCase("Sample.cs B.<Destructor>", "Destructor.txt")]
        [TestCase("Sample.cs <Destructor>", "Destructor.txt")]

        // Match generic class
        [TestCase("Sample.cs NS2.NS2.NS3.C{T,U}", "GenericClass.txt")]
        [TestCase("Sample.cs NS2.NS3.C{T,U}", "GenericClass.txt")]
        [TestCase("Sample.cs C{T,U}", "GenericClass.txt")]

        // Match generic method
        [TestCase("Sample.cs NS2.NS2.NS3.C{T,U}.CMethod{X,Y}", "GenericMethod.txt")]
        [TestCase("Sample.cs NS2.NS3.C{T,U}.CMethod{X,Y}", "GenericMethod.txt")]
        [TestCase("Sample.cs C{T,U}.CMethod{X,Y}", "GenericMethod.txt")]
        [TestCase("Sample.cs CMethod{X,Y}", "GenericMethod.txt")]

        // Aggregate namespaces
        [TestCase("Sample.cs NS2.NS3", "NS2NS3.txt")]

        // Aggregate classes
        [TestCase("Sample.cs NS2.NS3.A", "A.txt")]
        [TestCase("Sample.cs A", "A.txt")]

        // Funky scenario
        [TestCase("NeedCleanup.cs", "NeedCleanup.txt")]
        [TestCase("NeedCleanup.cs NeedCleanup", "NeedCleanupClass.txt")]
        [TestCase("Empty.cs", "Empty.txt")]

        // Match block structure only
        [TestCase("Options.cs =Options", "BlockOnlyClass.txt")]
        [TestCase("Options.cs =Options.Method", "BlockOnlyMethod.txt")]
        [TestCase("Options.cs =EmptyMethod", "BlockOnlyEmptyMethod.txt")]
        [TestCase("Options.cs =Options.Property", "BlockOnlyProperty.txt")]
        [TestCase("Options.cs =Options.Event", "BlockOnlyEvent.txt")]
        [TestCase("Options.cs =Options.Event.add", "BlockOnlyEventadd.txt")]
        [TestCase("Options.cs =Event.add", "BlockOnlyEventadd.txt")]
        [TestCase("Options.cs =add", "BlockOnlyEventadd.txt")]
        
        // Match content only
        [TestCase("Options.cs -Options", "ContentOnlyClass.txt")]
        [TestCase("Options.cs -Options.Method", "ContentOnlyMethod.txt")]
        [TestCase("Options.cs -Options.Property", "ContentOnlyProperty.txt")]
        [TestCase("Options.cs -Options.Event", "ContentOnlyEvent.txt")]
        [TestCase("Options.cs -Options.Event.add", "ContentOnlyEventadd.txt")]
        [TestCase("Options.cs -Event.add", "ContentOnlyEventadd.txt")]
        [TestCase("Options.cs -add", "ContentOnlyEventadd.txt")]

        // Match empty content
        [TestCase("Options.cs -remove", "Empty.txt")]
        [TestCase("Options.cs -get", "Empty.txt")]
        [TestCase("Options.cs -EmptyMethod", "Empty.txt")]
        public void ExtractSnippet(string pattern, string expectedFile)
        {
            // Run the extraction
            CSharpSnippetExtractor extractor = new CSharpSnippetExtractor(pattern, this.SourceDirectory);
            Projbook.Core.Model.Snippet snippet = extractor.Extract();

            // Load the expected file content
            MemoryStream memoryStream = new MemoryStream();
            using (var fileReader = new StreamReader(new FileStream(Path.GetFullPath(Path.Combine("Resources", "Expected", expectedFile)), FileMode.Open)))
            using (var fileWriter = new StreamWriter(memoryStream))
            {
                fileWriter.Write(fileReader.ReadToEnd());
            }

            // Assert
            Assert.AreEqual(
                System.Text.Encoding.UTF8.GetString(memoryStream.ToArray()).Replace("\r\n", Environment.NewLine),
                snippet.Content.Replace("\r\n", Environment.NewLine));
        }

        /// <summary>
        /// Tests extract snippet with invalid rule.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        [Test]
        [TestCase("abc abc(abc")]
        [TestCase("")]
        [TestCase(null)]
        [ExpectedException(ExpectedException = typeof(SnippetExtractionException), ExpectedMessage = "Invalid extraction rule")]
        public void ExtractSnippetInvalidRule(string pattern)
        {
            // Run the extraction
            CSharpSnippetExtractor extractor = new CSharpSnippetExtractor(pattern, this.SourceDirectory);
            Projbook.Core.Model.Snippet snippet = extractor.Extract();
        }

        /// <summary>
        /// Tests extract snippet with non matching member.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        [Test]
        [TestCase("Sample.cs DoesntExist")]
        [ExpectedException(ExpectedException = typeof(SnippetExtractionException), ExpectedMessage = "Cannot find member")]
        public void ExtractSnippetNotFound(string pattern)
        {
            // Run the extraction
            CSharpSnippetExtractor extractor = new CSharpSnippetExtractor(pattern, this.SourceDirectory);
            Projbook.Core.Model.Snippet snippet = extractor.Extract();
        }
    }
}