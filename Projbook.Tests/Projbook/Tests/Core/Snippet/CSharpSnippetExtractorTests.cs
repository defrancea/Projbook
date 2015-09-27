using NUnit.Framework;
using Projbook.Core.Snippet;
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