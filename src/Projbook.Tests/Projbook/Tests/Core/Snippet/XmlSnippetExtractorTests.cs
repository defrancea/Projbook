using NUnit.Framework;
using Projbook.Extension.Exception;
using Projbook.Extension.XmlExtractor;
using Projbook.Tests.Resources;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace Projbook.Tests.Core.Snippet
{
    /// <summary>
    /// Tests <see cref="XmlSnippetExtractor"/>.
    /// </summary>
    [TestFixture]
    public class XmlSnippetExtractorTests
    {
        /// <summary>
        /// Represents a file system abstraction.
        /// </summary>
        public IFileSystem FileSystem { get; private set; }

        /// <summary>
        /// Initializes the test.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Mock file system
            this.FileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "Source/Simple.xml", new MockFileData(SourceXmlFiles.Simple) },
                { "Source/SimpleNoNamespace.xml", new MockFileData(SourceXmlFiles.SimpleNoNamespace) },
                { "Source/Core.csproj", new MockFileData(SourceXmlFiles.CoreCsproj) },
                { "Expected/Simple.txt", new MockFileData(SourceXmlFiles.Simple) },
                { "Expected/SimpleNoNamespace.txt", new MockFileData(SourceXmlFiles.SimpleNoNamespace) },
                { "Expected/FirstLevel.txt", new MockFileData(ExpectedXmlFiles.FirstLevel) },
                { "Expected/SecondLevel.txt", new MockFileData(ExpectedXmlFiles.SecondLevel) },
                { "Expected/FirstLevelNoNamespace.txt", new MockFileData(ExpectedXmlFiles.FirstLevelNoNamespace) },
                { "Expected/FirstLevelWithNamespace.txt", new MockFileData(ExpectedXmlFiles.FirstLevelWithNamespace) },
                { "Expected/SecondLevelWithNamespace.txt", new MockFileData(ExpectedXmlFiles.SecondLevelWithNamespace) },
                { "Expected/Core.txt", new MockFileData(ExpectedXmlFiles.Core) },
            });
        }

        /// <summary>
        /// Tests extract snippet.
        /// </summary>
        /// <param name="fileName">The source file name.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="expectedFile">The expected file name.</param>
        [Test]

        // Whole file
        [TestCase("Source/SimpleNoNamespace.xml", "", "Expected/SimpleNoNamespace.txt")]
        [TestCase("Source/Simple.xml", "", "Expected/Simple.txt")]

        // Member selection with no namespace
        [TestCase("Source/SimpleNoNamespace.xml", "//FirstLevel", "Expected/FirstLevel.txt")]
        [TestCase("Source/SimpleNoNamespace.xml", "//SecondLevel", "Expected/SecondLevel.txt")]

        // Member selection with namespace
        [TestCase("Source/Simple.xml", "//FirstLevelNoNamespace", "Expected/FirstLevelNoNamespace.txt")]
        [TestCase("Source/Simple.xml", "//n:FirstLevelWithNamespace", "Expected/FirstLevelWithNamespace.txt")]
        [TestCase("Source/Simple.xml", "//m:SecondLevelWithNamespace", "Expected/SecondLevelWithNamespace.txt")]

        // Actual file extraction
        [TestCase("Source/Core.csproj", "//ProjectGuid", "Expected/Core.txt")]
        public void ExtractSnippet(string fileName, string pattern, string expectedFile)
        {
            // Run the extraction
            Extension.Model.PlainTextSnippet snippet = new XmlSnippetExtractor().Extract(this.FileSystem.FileInfo.FromFileName(fileName), pattern) as Extension.Model.PlainTextSnippet;

            // Assert
            Assert.AreEqual(this.FileSystem.File.ReadAllText(expectedFile), snippet.Text.Replace("\r\n", "\n"));
        }

        /// <summary>
        /// Tests extract snippet with invalid rule.
        /// </summary>
        [Test]
        public void ExtractSnippetInvalidRule()
        {
            // Run the extraction
            Assert.Throws(
                Is.TypeOf<SnippetExtractionException>().And.Message.EqualTo("Invalid extraction rule"),
                () => new XmlSnippetExtractor().Extract(this.FileSystem.FileInfo.FromFileName("Source/Simple.xml"), "abc abc(abc"));
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
                () => new XmlSnippetExtractor().Extract(this.FileSystem.FileInfo.FromFileName("Source/Simple.xml"), "//DoesntExist"));
        }
    }
}