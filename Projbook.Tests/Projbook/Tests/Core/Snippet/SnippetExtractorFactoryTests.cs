using NUnit.Framework;
using Projbook.Core.Snippet;
using Projbook.Core.Snippet.CSharp;
using System;
using System.IO;

namespace Projbook.Tests.Core.Snippet
{
    /// <summary>
    /// Tests <see cref="SnippetExtractorFactory"/>.
    /// </summary>
    [TestFixture]
    public class SnippetExtractorFactoryTests : AbstractSnippetTests
    {
        /// <summary>
        /// The tested extractor factory.
        /// </summary>
        public SnippetExtractorFactory SnippetExtractorFactory { get; private set; }

        /// <summary>
        /// Initializes the test.
        /// </summary>
        [SetUp]
        public override void Setup()
        {
            // Initialize extractor
            base.Setup();
            this.SnippetExtractorFactory = new SnippetExtractorFactory(this.CsprojFile);
        }

        /// <summary>
        /// Tests with invalid input.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WrongInitSourceNull()
        {
            new SnippetExtractorFactory(null);
        }

        /// <summary>
        /// Tests with invalid input.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void WrongInitSourceEmpty()
        {
            new SnippetExtractorFactory(new FileInfo(""));
        }

        /// <summary>
        /// Tests extract CreateExstractor.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateEmpty()
        {
            this.SnippetExtractorFactory.CreateExtractor(null);
        }

        /// <summary>
        /// Tests extract CreateExstractor.
        /// </summary>
        [Test]
        [TestCase]
        public void CreateExtractorUnknownLanguage()
        {
            // Process
            Assert.IsNull(this.SnippetExtractorFactory.CreateExtractor("foo"));
        }

        /// <summary>
        /// Tests extract CreateExstractor.
        /// </summary>
        [Test]
        [TestCase]
        public void CreateExtractorKnownLanguageNoSnippet()
        {
            // Process
            Assert.IsNull(this.SnippetExtractorFactory.CreateExtractor("csharp"));
        }

        /// <summary>
        /// Tests extract CreateExstractor.
        /// </summary>
        [Test]
        [TestCase]
        public void CreateExtractorKnownLanguageCSharpEmptySnippet()
        {
            // Process
            Assert.IsNull(this.SnippetExtractorFactory.CreateExtractor("csharp[]"));
        }

        /// <summary>
        /// Tests extract CreateExstractor.
        /// </summary>
        [Test]
        [TestCase]
        public void CreateExtractorKnownLanguageCSharpSnippet()
        {
            // Process
            CSharpSnippetExtractor extractor = this.SnippetExtractorFactory.CreateExtractor("csharp[File.cs]") as CSharpSnippetExtractor;
            Assert.IsNotNull(extractor);
            Assert.AreEqual("File.cs", extractor.Pattern);
        }
    }
}