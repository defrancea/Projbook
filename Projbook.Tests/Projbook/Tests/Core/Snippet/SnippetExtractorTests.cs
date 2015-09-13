using NUnit.Framework;
using Projbook.Core.Model;
using Projbook.Core.Snippet;
using System;
using System.IO;

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
            this.Extractor = new SnippetExtractor(new DirectoryInfo(Path.Combine(".", "Resources", "SourcesA")));
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
                "namespace Projbook.Tests.Resources.SourcesA\r\n{\r\n    public class AnyClass\r\n    {\r\n    }\r\n}",
                snippet.Content);
        }
    }
}