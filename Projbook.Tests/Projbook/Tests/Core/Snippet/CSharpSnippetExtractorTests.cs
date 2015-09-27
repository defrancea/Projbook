using NUnit.Framework;
using Projbook.Core.Snippet;
using System;
using System.IO;
using System.Reflection;

namespace Projbook.Tests.Core
{
    /// <summary>
    /// Tests <see cref="CSharpSnippetExtractor"/>.
    /// </summary>
    [TestFixture]
    public class CSharpSnippetExtractorTests
    {
        /// <summary>
        /// The tested extractor.
        /// </summary>
        public DirectoryInfo SourceDirectory { get; private set; }

        /// <summary>
        /// Initializes the test.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Initialize extractor
            string testAssemblyLocation = Assembly.GetAssembly(typeof(CSharpSnippetExtractorTests)).Location;
            string testAssemblyDirectory = Path.GetDirectoryName(testAssemblyLocation);
            string testSourceLocation = Path.GetFullPath(Path.Combine(testAssemblyDirectory, "..", ".."));
            this.SourceDirectory = new DirectoryInfo(Path.Combine(testSourceLocation, "Resources", "SourcesA"));
        }

        /// <summary>
        /// Tests with invalid input.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void WrongInitSource()
        {
            new CSharpSnippetExtractor("Foo.cs");
            new CSharpSnippetExtractor("Foo.cs", new DirectoryInfo[0]);
        }

        /// <summary>
        /// Tests with invalid input.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void WrongInitEmpty()
        {
            new CSharpSnippetExtractor(null, new DirectoryInfo[] { new DirectoryInfo("Foo") });
            new CSharpSnippetExtractor(string.Empty, new DirectoryInfo[] { new DirectoryInfo("Foo") });
            new CSharpSnippetExtractor("   ", new DirectoryInfo[] { new DirectoryInfo("Foo") });
        }

        /// <summary>
        /// Tests extract whole file.
        /// </summary>
        [Test]
        [TestCase]
        public void ExtractWholeFile()
        {
            // Process
            CSharpSnippetExtractor extractor = new CSharpSnippetExtractor("AnyClass.cs", this.SourceDirectory);
            Projbook.Core.Model.Razor.Snippet snippet = extractor.Extract();

            // Assert
            Assert.AreEqual(
                string.Format("namespace Projbook.Tests.Resources.SourcesA{0}{{{0}    public class AnyClass{0}    {{{0}    }}{0}}}", Environment.NewLine),
                snippet.Content);
        }
    }
}