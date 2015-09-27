using NUnit.Framework;
using Projbook.Core.Model;
using Projbook.Core.Snippet;
using System;
using System.IO;
using System.Reflection;

namespace Projbook.Tests.Core
{
    /// <summary>
    /// Tests <see cref="SnippetExtractor"/>.
    /// </summary>
    [TestFixture]
    public class SnippetExtractorTests
    {
        /// <summary>
        /// The tested extractor.
        /// </summary>
        public SnippetExtractor Extractor { get; private set; }

        /// <summary>
        /// Initializes the test.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Initialize extractor
            string testAssemblyLocation = Assembly.GetAssembly(typeof(SnippetExtractorTests)).Location;
            string testAssemblyDirectory = Path.GetDirectoryName(testAssemblyLocation);
            string testSourceLocation = Path.GetFullPath(Path.Combine(testAssemblyDirectory, "..", ".."));
            this.Extractor = new SnippetExtractor(new DirectoryInfo(Path.Combine(testSourceLocation, "Resources", "SourcesA")));
        }

        /// <summary>
        /// Tests with invalid input.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WrongInitNull()
        {
            new SnippetExtractor(null);
        }

        /// <summary>
        /// Tests with invalid input.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void WrongInitEmpty()
        {
            new SnippetExtractor(new DirectoryInfo[0]);
        }

        /// <summary>
        /// Tests extract whole file.
        /// </summary>
        [Test]
        [TestCase]
        public void ExtractWholeFile()
        {
            // Process
            Snippet snippet = this.Extractor.Extract("AnyClass.cs");

            // Assert
            Assert.IsNotNull(snippet.Rule);
            Assert.IsNotNull(
                Path.Combine(this.Extractor.SourceDictionaries[0].FullName, "AnyClass.cs"),
                snippet.Rule.TargetFile.FullName);

            Assert.AreEqual(
                string.Format("namespace Projbook.Tests.Resources.SourcesA{0}{{{0}    public class AnyClass{0}    {{{0}    }}{0}}}", Environment.NewLine),
                snippet.Content);
        }
    }
}