using NUnit.Framework;
using Projbook.Extension.CSharpExtractor;
using Projbook.Extension.Exception;
using Projbook.Extension.Spi;
using Projbook.Tests.Resources;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace Projbook.Tests.Core.Snippet
{
    /// <summary>
    /// Tests <see cref="CSharpSnippetExtractor"/>.
    /// </summary>
    [TestFixture]
    public class CSharpSnippetExtractorTests
    {
        /// <summary>
        /// Represents a file system abstraction.
        /// </summary>
        public IFileSystem FileSystem { get; private set; }

        /// <summary>
        /// Use a cache for unit testing in order to speed up execution and simulate an actual usage.
        /// </summary>
        private Dictionary<string, ISnippetExtractor> extractorCache = new Dictionary<string, ISnippetExtractor>();

        /// <summary>
        /// Initializes the test.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Mock file system
            this.FileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "Source/AnyClass.cs", new MockFileData(SourceCSharpFiles.AnyClass) },
                { "Source/Sample.cs", new MockFileData(SourceCSharpFiles.Sample) },
                { "Source/NeedCleanup.cs", new MockFileData(SourceCSharpFiles.NeedCleanup) },
                { "Source/Empty.cs", new MockFileData(SourceCSharpFiles.Empty) },
                { "Source/Options.cs", new MockFileData(SourceCSharpFiles.Options) },
                { "Expected/AnyClass.txt", new MockFileData(ExpectedCSharpFiles.AnyClass) },
                { "Expected/NS.txt", new MockFileData(ExpectedCSharpFiles.NS) },
                { "Expected/OneClassSomewhere.txt", new MockFileData(ExpectedCSharpFiles.OneClassSomewhere) },
                { "Expected/I.txt", new MockFileData(ExpectedCSharpFiles.I) },
                { "Expected/SubClass.txt", new MockFileData(ExpectedCSharpFiles.SubClass) },
                { "Expected/FieldSomewhere.txt", new MockFileData(ExpectedCSharpFiles.FieldSomewhere) },
                { "Expected/ArrayField.txt", new MockFileData(ExpectedCSharpFiles.ArrayField) },
                { "Expected/WhateverProperty.txt", new MockFileData(ExpectedCSharpFiles.WhateverProperty) },
                { "Expected/WhateverPropertyget.txt", new MockFileData(ExpectedCSharpFiles.WhateverPropertyget) },
                { "Expected/WhateverPropertyset.txt", new MockFileData(ExpectedCSharpFiles.WhateverPropertyset) },
                { "Expected/Indexer.txt", new MockFileData(ExpectedCSharpFiles.Indexer) },
                { "Expected/Indexerget.txt", new MockFileData(ExpectedCSharpFiles.Indexerget) },
                { "Expected/Indexerset.txt", new MockFileData(ExpectedCSharpFiles.Indexerset) },
                { "Expected/get.txt", new MockFileData(ExpectedCSharpFiles.get) },
                { "Expected/set.txt", new MockFileData(ExpectedCSharpFiles.set) },
                { "Expected/Event.txt", new MockFileData(ExpectedCSharpFiles.Event) },
                { "Expected/Eventadd.txt", new MockFileData(ExpectedCSharpFiles.Eventadd) },
                { "Expected/Eventremove.txt", new MockFileData(ExpectedCSharpFiles.Eventremove) },
                { "Expected/Foo.txt", new MockFileData(ExpectedCSharpFiles.Foo) },
                { "Expected/FooString.txt", new MockFileData(ExpectedCSharpFiles.FooString) },
                { "Expected/FooStringInt.txt", new MockFileData(ExpectedCSharpFiles.FooStringInt) },
                { "Expected/IMethod.txt", new MockFileData(ExpectedCSharpFiles.IMethod) },
                { "Expected/NSNS2NS3.txt", new MockFileData(ExpectedCSharpFiles.NSNS2NS3) },
                { "Expected/NSNS2NS3A.txt", new MockFileData(ExpectedCSharpFiles.NSNS2NS3A) },
                { "Expected/NS2.txt", new MockFileData(ExpectedCSharpFiles.NS2) },
                { "Expected/NS2NS2NS3.txt", new MockFileData(ExpectedCSharpFiles.NS2NS2NS3) },
                { "Expected/NS2NS2NS3A.txt", new MockFileData(ExpectedCSharpFiles.NS2NS2NS3A) },
                { "Expected/Constructor.txt", new MockFileData(ExpectedCSharpFiles.Constructor) },
                { "Expected/Destructor.txt", new MockFileData(ExpectedCSharpFiles.Destructor) },
                { "Expected/GenericClass.txt", new MockFileData(ExpectedCSharpFiles.GenericClass) },
                { "Expected/GenericMethod.txt", new MockFileData(ExpectedCSharpFiles.GenericMethod) },
                { "Expected/NS2NS3.txt", new MockFileData(ExpectedCSharpFiles.NS2NS3) },
                { "Expected/A.txt", new MockFileData(ExpectedCSharpFiles.A) },
                { "Expected/E.txt", new MockFileData(ExpectedCSharpFiles.E) },
                { "Expected/EA.txt", new MockFileData("EA") },
                { "Expected/EC.txt", new MockFileData("EC") },
                { "Expected/NeedCleanup.txt", new MockFileData(ExpectedCSharpFiles.NeedCleanup) },
                { "Expected/NeedCleanupClass.txt", new MockFileData(ExpectedCSharpFiles.NeedCleanupClass) },
                { "Expected/Empty.txt", new MockFileData(ExpectedCSharpFiles.Empty) },
                { "Expected/BlockOnlyClass.txt", new MockFileData(ExpectedCSharpFiles.BlockOnlyClass) },
                { "Expected/BlockOnlyMethod.txt", new MockFileData(ExpectedCSharpFiles.BlockOnlyMethod) },
                { "Expected/BlockOnlyEmptyMethod.txt", new MockFileData(ExpectedCSharpFiles.BlockOnlyEmptyMethod) },
                { "Expected/BlockOnlyProperty.txt", new MockFileData(ExpectedCSharpFiles.BlockOnlyProperty) },
                { "Expected/BlockOnlyEvent.txt", new MockFileData(ExpectedCSharpFiles.BlockOnlyEvent) },
                { "Expected/BlockOnlyEventadd.txt", new MockFileData(ExpectedCSharpFiles.BlockOnlyEventadd) },
                { "Expected/BlockOnlyEscapedMethod.txt", new MockFileData(ExpectedCSharpFiles.BlockOnlyEscapedMethod) },
                { "Expected/BlockOnlyEscapedMethod2.txt", new MockFileData(ExpectedCSharpFiles.BlockOnlyEscapedMethod2) },
                { "Expected/BlockOnlyEscapedClass.txt", new MockFileData(ExpectedCSharpFiles.BlockOnlyEscapedClass) },
                { "Expected/BlockOnlyEscapedClass2.txt", new MockFileData(ExpectedCSharpFiles.BlockOnlyEscapedClass2) },
                { "Expected/ContentOnlyClass.txt", new MockFileData(ExpectedCSharpFiles.ContentOnlyClass) },
                { "Expected/ContentOnlyMethod.txt", new MockFileData(ExpectedCSharpFiles.ContentOnlyMethod) },
                { "Expected/ContentOnlyProperty.txt", new MockFileData(ExpectedCSharpFiles.ContentOnlyProperty) },
                { "Expected/ContentOnlyEvent.txt", new MockFileData(ExpectedCSharpFiles.ContentOnlyEvent) },
                { "Expected/ContentOnlyEventadd.txt", new MockFileData(ExpectedCSharpFiles.ContentOnlyEventadd) },
                { "Expected/ContentOnlyEscapedClass.txt", new MockFileData(ExpectedCSharpFiles.ContentOnlyEscapedClass) },
                { "Expected/ContentOnlyEscapedClass2.txt", new MockFileData(ExpectedCSharpFiles.ContentOnlyEscapedClass2) },
                { "Expected/ContentOnlyEscapedMethod.txt", new MockFileData(ExpectedCSharpFiles.ContentOnlyEscapedMethod) }
            });
        }

        /// <summary>
        /// Tests extract snippet.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="expectedFile">The expected file.</param>
        [Test]

        // Whole file
        [TestCase("Source/AnyClass.cs", "", "Expected/AnyClass.txt")]
        [TestCase("Source/AnyClass.cs", null, "Expected/AnyClass.txt")]
        [TestCase("Source/AnyClass.cs", "   ", "Expected/AnyClass.txt")]

        // Simple matching
        [TestCase("Source/Sample.cs", "NS", "Expected/NS.txt")]

        // Match class
        [TestCase("Source/Sample.cs", "NS.OneClassSomewhere", "Expected/OneClassSomewhere.txt")]
        [TestCase("Source/Sample.cs", "OneClassSomewhere", "Expected/OneClassSomewhere.txt")]

        // Match interface
        [TestCase("Source/Sample.cs", "NS2.NS2.NS3.I", "Expected/I.txt")]
        [TestCase("Source/Sample.cs", "I", "Expected/I.txt")]

        // Match enum
        [TestCase("Source/Sample.cs", "NS2.NS3.E", "Expected/E.txt")]
        [TestCase("Source/Sample.cs", "E", "Expected/E.txt")]

        // Match enum member
        [TestCase("Source/Sample.cs", "EA", "Expected/EA.txt")]
        [TestCase("Source/Sample.cs", "E.EA", "Expected/EA.txt")]
        [TestCase("Source/Sample.cs", "EC", "Expected/EC.txt")]
        [TestCase("Source/Sample.cs", "E.EC", "Expected/EC.txt")]

        // Match subclass
        [TestCase("Source/Sample.cs", "NS.OneClassSomewhere.SubClass", "Expected/SubClass.txt")]
        [TestCase("Source/Sample.cs", "OneClassSomewhere.SubClass", "Expected/SubClass.txt")]
        [TestCase("Source/Sample.cs", "SubClass", "Expected/SubClass.txt")]

        // Match field
        [TestCase("Source/Sample.cs", "NS2.NS2.NS3.WithField.aFieldSomewhere", "Expected/FieldSomewhere.txt")]
        [TestCase("Source/Sample.cs", "WithField.aFieldSomewhere", "Expected/FieldSomewhere.txt")]
        [TestCase("Source/Sample.cs", "aFieldSomewhere", "Expected/FieldSomewhere.txt")]
        [TestCase("Source/Sample.cs", "NS2.NS2.NS3.WithField.anotherFieldSomewhere", "Expected/FieldSomewhere.txt")]
        [TestCase("Source/Sample.cs", "WithField.anotherFieldSomewhere", "Expected/FieldSomewhere.txt")]
        [TestCase("Source/Sample.cs", "anotherFieldSomewhere", "Expected/FieldSomewhere.txt")]
        [TestCase("Source/Sample.cs", "NS2.NS2.NS3.WithField.fieldArray", "Expected/ArrayField.txt")]
        [TestCase("Source/Sample.cs", "WithField.fieldArray", "Expected/ArrayField.txt")]
        [TestCase("Source/Sample.cs", "fieldArray", "Expected/ArrayField.txt")]

        // Match property
        [TestCase("Source/Sample.cs", "NS.OneClassSomewhere.SubClass.WhateverProperty", "Expected/WhateverProperty.txt")]
        [TestCase("Source/Sample.cs", "OneClassSomewhere.SubClass.WhateverProperty", "Expected/WhateverProperty.txt")]
        [TestCase("Source/Sample.cs", "SubClass.WhateverProperty", "Expected/WhateverProperty.txt")]
        [TestCase("Source/Sample.cs", "WhateverProperty", "Expected/WhateverProperty.txt")]

        // Match property getter
        [TestCase("Source/Sample.cs", "NS.OneClassSomewhere.SubClass.WhateverProperty.get", "Expected/WhateverPropertyget.txt")]
        [TestCase("Source/Sample.cs", "OneClassSomewhere.SubClass.WhateverProperty.get", "Expected/WhateverPropertyget.txt")]
        [TestCase("Source/Sample.cs", "SubClass.WhateverProperty.get", "Expected/WhateverPropertyget.txt")]
        [TestCase("Source/Sample.cs", "WhateverProperty.get", "Expected/WhateverPropertyget.txt")]

        // Match property setter
        [TestCase("Source/Sample.cs", "NS.OneClassSomewhere.SubClass.WhateverProperty.set", "Expected/WhateverPropertyset.txt")]
        [TestCase("Source/Sample.cs", "OneClassSomewhere.SubClass.WhateverProperty.set", "Expected/WhateverPropertyset.txt")]
        [TestCase("Source/Sample.cs", "SubClass.WhateverProperty.set", "Expected/WhateverPropertyset.txt")]
        [TestCase("Source/Sample.cs", "WhateverProperty.set", "Expected/WhateverPropertyset.txt")]

        // Match indexer
        [TestCase("Source/Sample.cs", "NS2.NS2.NS3.D.[string,int]", "Expected/Indexer.txt")]
        [TestCase("Source/Sample.cs", "NS2.NS3.D.[string,int]", "Expected/Indexer.txt")]
        [TestCase("Source/Sample.cs", "D.[string,int]", "Expected/Indexer.txt")]
        [TestCase("Source/Sample.cs", "[string,int]", "Expected/Indexer.txt")]

        // Match indexer getter
        [TestCase("Source/Sample.cs", "NS2.NS2.NS3.D.[string,int].get", "Expected/Indexerget.txt")]
        [TestCase("Source/Sample.cs", "NS2.NS3.D.[string,int].get", "Expected/Indexerget.txt")]
        [TestCase("Source/Sample.cs", "D.[string,int].get", "Expected/Indexerget.txt")]
        [TestCase("Source/Sample.cs", "[string,int].get", "Expected/Indexerget.txt")]

        // Match indexer setter
        [TestCase("Source/Sample.cs", "NS2.NS2.NS3.D.[string,int].set", "Expected/Indexerset.txt")]
        [TestCase("Source/Sample.cs", "NS2.NS3.D.[string,int].set", "Expected/Indexerset.txt")]
        [TestCase("Source/Sample.cs", "D.[string,int].set", "Expected/Indexerset.txt")]
        [TestCase("Source/Sample.cs", "[string,int].set", "Expected/Indexerset.txt")]

        // Match all getter and setter
        [TestCase("Source/Sample.cs", "set", "Expected/set.txt")]
        [TestCase("Source/Sample.cs", "get", "Expected/get.txt")]

        // Match event
        [TestCase("Source/Sample.cs", "NS2.NS2.NS3.D.Event", "Expected/Event.txt")]
        [TestCase("Source/Sample.cs", "NS2.NS3.D.Event", "Expected/Event.txt")]
        [TestCase("Source/Sample.cs", "D.Event", "Expected/Event.txt")]
        [TestCase("Source/Sample.cs", "Event", "Expected/Event.txt")]

        // Match event adder
        [TestCase("Source/Sample.cs", "NS2.NS2.NS3.D.Event.add", "Expected/Eventadd.txt")]
        [TestCase("Source/Sample.cs", "NS2.NS3.D.Event.add", "Expected/Eventadd.txt")]
        [TestCase("Source/Sample.cs", "D.Event.add", "Expected/Eventadd.txt")]
        [TestCase("Source/Sample.cs", "Event.add", "Expected/Eventadd.txt")]
        [TestCase("Source/Sample.cs", "add", "Expected/Eventadd.txt")]

        // Match event remover
        [TestCase("Source/Sample.cs", "NS2.NS2.NS3.D.Event.remove", "Expected/Eventremove.txt")]
        [TestCase("Source/Sample.cs", "NS2.NS3.D.Event.remove", "Expected/Eventremove.txt")]
        [TestCase("Source/Sample.cs", "D.Event.remove", "Expected/Eventremove.txt")]
        [TestCase("Source/Sample.cs", "Event.remove", "Expected/Eventremove.txt")]
        [TestCase("Source/Sample.cs", "remove", "Expected/Eventremove.txt")]

        // Match method with overloads
        [TestCase("Source/Sample.cs", "NS.OneClassSomewhere.Foo", "Expected/Foo.txt")]
        [TestCase("Source/Sample.cs", "OneClassSomewhere.Foo", "Expected/Foo.txt")]
        [TestCase("Source/Sample.cs", "Foo", "Expected/Foo.txt")]

        // Match method signature
        [TestCase("Source/Sample.cs", "NS.OneClassSomewhere.Foo(string)", "Expected/FooString.txt")]
        [TestCase("Source/Sample.cs", "OneClassSomewhere.Foo(string)", "Expected/FooString.txt")]
        [TestCase("Source/Sample.cs", "Foo(string)", "Expected/FooString.txt")]
        [TestCase("Source/Sample.cs", "(string)", "Expected/FooString.txt")]
        [TestCase("Source/Sample.cs", "NS.OneClassSomewhere.Foo(string, int)", "Expected/FooStringInt.txt")]
        [TestCase("Source/Sample.cs", "OneClassSomewhere.Foo(string, int)", "Expected/FooStringInt.txt")]
        [TestCase("Source/Sample.cs", "Foo(string, int)", "Expected/FooStringInt.txt")]
        [TestCase("Source/Sample.cs", "(string, int)", "Expected/FooStringInt.txt")]
        [TestCase("Source/Sample.cs", "(bool, string, int)", "Expected/IMethod.txt")]
        [TestCase("Source/Sample.cs", "NS2.NS2.NS3.I.InterfaceMethod(bool, string, int)", "Expected/IMethod.txt")]
        [TestCase("Source/Sample.cs", "InterfaceMethod", "Expected/IMethod.txt")]

        // Match sub namespace with overlapping
        [TestCase("Source/Sample.cs", "NS.NS2.NS3", "Expected/NSNS2NS3.txt")]
        [TestCase("Source/Sample.cs", "NS.NS2.NS3.A", "Expected/NSNS2NS3A.txt")]
        [TestCase("Source/Sample.cs", "NS2", "Expected/NS2.txt")]
        [TestCase("Source/Sample.cs", "NS2.NS2.NS3", "Expected/NS2NS2NS3.txt")]
        [TestCase("Source/Sample.cs", "NS2.NS2.NS3.A", "Expected/NS2NS2NS3A.txt")]

        // Match constructor
        [TestCase("Source/Sample.cs", "NS2.NS2.NS3.B.<Constructor>", "Expected/Constructor.txt")]
        [TestCase("Source/Sample.cs", "NS2.NS3.B.<Constructor>", "Expected/Constructor.txt")]
        [TestCase("Source/Sample.cs", "B.<Constructor>", "Expected/Constructor.txt")]
        [TestCase("Source/Sample.cs", "<Constructor>", "Expected/Constructor.txt")]

        // Match destructor
        [TestCase("Source/Sample.cs", "NS2.NS2.NS3.B.<Destructor>", "Expected/Destructor.txt")]
        [TestCase("Source/Sample.cs", "NS2.NS3.B.<Destructor>", "Expected/Destructor.txt")]
        [TestCase("Source/Sample.cs", "B.<Destructor>", "Expected/Destructor.txt")]
        [TestCase("Source/Sample.cs", "<Destructor>", "Expected/Destructor.txt")]

        // Match generic class
        [TestCase("Source/Sample.cs", "NS2.NS2.NS3.C{T, U}", "Expected/GenericClass.txt")]
        [TestCase("Source/Sample.cs", "NS2.NS3.C{T, U}", "Expected/GenericClass.txt")]
        [TestCase("Source/Sample.cs", "C{T, U}", "Expected/GenericClass.txt")]

        // Match generic method
        [TestCase("Source/Sample.cs", "NS2.NS2.NS3.C{T, U}.CMethod{X, Y}", "Expected/GenericMethod.txt")]
        [TestCase("Source/Sample.cs", "NS2.NS3.C{T, U}.CMethod{X, Y}", "Expected/GenericMethod.txt")]
        [TestCase("Source/Sample.cs", "C{T, U}.CMethod{X, Y}", "Expected/GenericMethod.txt")]
        [TestCase("Source/Sample.cs", "CMethod{X, Y}", "Expected/GenericMethod.txt")]

        // Aggregate namespaces
        [TestCase("Source/Sample.cs", "NS2.NS3", "Expected/NS2NS3.txt")]

        // Aggregate classes
        [TestCase("Source/Sample.cs", "NS2.NS3.A", "Expected/A.txt")]
        [TestCase("Source/Sample.cs", "A", "Expected/A.txt")]

        // Funky scenario
        [TestCase("Source/NeedCleanup.cs", "", "Expected/NeedCleanup.txt")]
        [TestCase("Source/NeedCleanup.cs", "NeedCleanup", "Expected/NeedCleanupClass.txt")]
        [TestCase("Source/Empty.cs", "", "Expected/Empty.txt")]

        // Match block structure only
        [TestCase("Source/Options.cs", "=Options", "Expected/BlockOnlyClass.txt")]
        [TestCase("Source/Options.cs", "=Options.Method", "Expected/BlockOnlyMethod.txt")]
        [TestCase("Source/Options.cs", "=EmptyMethod", "Expected/BlockOnlyEmptyMethod.txt")]
        [TestCase("Source/Options.cs", "=Options.Property", "Expected/BlockOnlyProperty.txt")]
        [TestCase("Source/Options.cs", "=Options.Event", "Expected/BlockOnlyEvent.txt")]
        [TestCase("Source/Options.cs", "=Options.Event.add", "Expected/BlockOnlyEventadd.txt")]
        [TestCase("Source/Options.cs", "=Event.add", "Expected/BlockOnlyEventadd.txt")]
        [TestCase("Source/Options.cs", "=add", "Expected/BlockOnlyEventadd.txt")]
        [TestCase("Source/Options.cs", "=ConflictingCommentsClass.Method", "Expected/BlockOnlyEscapedMethod.txt")]
        [TestCase("Source/Options.cs", "=ConflictingCommentsClass2.Method", "Expected/BlockOnlyEscapedMethod2.txt")]
        [TestCase("Source/Options.cs", "=ConflictingCommentsClass", "Expected/BlockOnlyEscapedClass.txt")]
        [TestCase("Source/Options.cs", "=ConflictingCommentsClass2", "Expected/BlockOnlyEscapedClass2.txt")]

        // Match content only
        [TestCase("Source/Options.cs", "-Options", "Expected/ContentOnlyClass.txt")]
        [TestCase("Source/Options.cs", "-Options.Method", "Expected/ContentOnlyMethod.txt")]
        [TestCase("Source/Options.cs", "-EmptyMethod", "Expected/Empty.txt")]
        [TestCase("Source/Options.cs", "-Options.Property", "Expected/ContentOnlyProperty.txt")]
        [TestCase("Source/Options.cs", "-Options.Event", "Expected/ContentOnlyEvent.txt")]
        [TestCase("Source/Options.cs", "-Options.Event.add", "Expected/ContentOnlyEventadd.txt")]
        [TestCase("Source/Options.cs", "-Event.add", "Expected/ContentOnlyEventadd.txt")]
        [TestCase("Source/Options.cs", "-add", "Expected/ContentOnlyEventadd.txt")]
        [TestCase("Source/Options.cs", "-ConflictingCommentsClass", "Expected/ContentOnlyEscapedClass.txt")]
        [TestCase("Source/Options.cs", "-ConflictingCommentsClass2", "Expected/ContentOnlyEscapedClass2.txt")]
        [TestCase("Source/Options.cs", "-ConflictingCommentsClass.Method", "Expected/ContentOnlyEscapedMethod.txt")]
        [TestCase("Source/Options.cs", "-ConflictingCommentsClass2.Method", "Expected/ContentOnlyEscapedMethod.txt")]
        
        // Match empty content
        [TestCase("Source/Options.cs", "-remove", "Expected/Empty.txt")]
        [TestCase("Source/Options.cs", "-get", "Expected/Empty.txt")]
        [TestCase("Source/Options.cs", "-EmptyMethod", "Expected/Empty.txt")]
        public void ExtractSnippet(string fileName, string pattern, string expectedFile)
        {
            // Run the extraction
            ISnippetExtractor snippetExtractor;
            if (!this.extractorCache.TryGetValue(fileName, out snippetExtractor))
            {
                snippetExtractor = new CSharpSnippetExtractor();
                this.extractorCache[fileName] = snippetExtractor;
            }
            Extension.Model.PlainTextSnippet snippet = snippetExtractor.Extract(this.FileSystem.FileInfo.FromFileName(fileName), pattern) as Extension.Model.PlainTextSnippet;

            // Assert
            expectedFile = expectedFile.Replace('/', this.FileSystem.Path.DirectorySeparatorChar);
            Assert.AreEqual(this.FileSystem.File.ReadAllText(expectedFile), snippet.Text.Replace("\r\n", "\n"));
        }

        /// <summary>
        /// Tests extract snippet with invalid rule.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        [Test]
        public void ExtractSnippetInvalidRule()
        {
            // Run the extraction
            Assert.Throws(
                Is.TypeOf<SnippetExtractionException>().And.Message.EqualTo("Invalid extraction rule"),
                () => new CSharpSnippetExtractor().Extract(this.FileSystem.FileInfo.FromFileName("Source/AnyClass.cs"), "abc abc(abc"));
        }

        /// <summary>
        /// Tests extract snippet with non matching member.
        /// </summary>
        [Test]
        public void ExtractSnippetNotFound()
        {
            // Run the extraction
            Assert.Throws(
                Is.TypeOf<SnippetExtractionException>().And.Message.EqualTo("Cannot find member"),
                () => new CSharpSnippetExtractor().Extract(this.FileSystem.FileInfo.FromFileName("Source/AnyClass.cs"), "DoesntExist"));
        }
    }
}