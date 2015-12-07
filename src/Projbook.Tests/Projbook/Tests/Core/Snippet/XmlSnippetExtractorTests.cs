using NUnit.Framework;
using Projbook.Core.Exception;
using Projbook.Core.Snippet;
using Projbook.Core.Snippet.Xml;
using System;
using System.Collections.Generic;
using System.IO;

namespace Projbook.Tests.Core.Snippet
{
    /// <summary>
    /// Tests <see cref="XmlSnippetExtractor"/>.
    /// </summary>
    [TestFixture]
    public class XmlSnippetExtractorTests : AbstractTests
    {
        /// <summary>
        /// Use a cache for unit testing in order to speed up execution and simulate an actual usage.
        /// </summary>
        private Dictionary<string, ISnippetExtractor> extractorCache = new Dictionary<string, ISnippetExtractor>();

        /// <summary>
        /// Tests with invalid input.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void WrongInitSourceDefaultDirectories()
        {
            new XmlSnippetExtractor().Extract("Foo.xml", string.Empty);
        }
        /// <summary>
        /// Tests with invalid input.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void WrongInitSourceEmptyDirectories()
        {
            new XmlSnippetExtractor(new DirectoryInfo[0]).Extract("Foo.xml", string.Empty);
        }

        /// <summary>
        /// Tests with invalid input.
        /// </summary>
        /// <param name="filePath">The file path to test.</param>
        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [ExpectedException(typeof(SnippetExtractionException))]
        public void WrongInitEmpty(string filePath)
        {
            new XmlSnippetExtractor(new DirectoryInfo("Foo")).Extract(filePath, string.Empty);
        }

        /// <summary>
        /// Tests extract snippet.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="expectedFile">The expected file.</param>
        [Test]

        // Whole file
        [TestCase("SampleNoNamespace.xml", "", "SampleXmlNoNamespaceWholeFile.txt")]
        [TestCase("Sample.xml", "", "SampleXmlWholeFile.txt")]

        // Member selection with no namespace
        [TestCase("SampleNoNamespace.xml", "//FirstLevel", "FirstLevel.txt")]
        [TestCase("SampleNoNamespace.xml", "//SecondLevel", "SecondLevel.txt")]

        // Member selection with namespace
        [TestCase("Sample.xml", "//FirstLevelNoNamespace", "FirstLevelNoNamespace.txt")]
        [TestCase("Sample.xml", "//n:FirstLevelWithNNamespace", "FirstLevelWithNNamespace.txt")]
        [TestCase("Sample.xml", "//m:SecondLevelWithMNamespace", "SecondLevelWithMNamespace.txt")]

        // Acrual file extraction
        [TestCase("Projbook.Core.csproj", "//ProjectGuid", "ProjectCoreXml.txt")]
        public void ExtractSnippet(string fileName, string pattern, string expectedFile)
        {
            // Resolve path
            if (!fileName.EndsWith("csproj"))
            {
                fileName = this.ComputeFilePath(fileName);
            }
            
            // Run the extraction
            ISnippetExtractor snippetExtractor;
            if (!this.extractorCache.TryGetValue(fileName, out snippetExtractor))
            {
                snippetExtractor = new XmlSnippetExtractor(this.SourceDirectories);
                this.extractorCache[fileName] = snippetExtractor;
            }
            Projbook.Core.Model.Snippet snippet = snippetExtractor.Extract(fileName, pattern);

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
        [ExpectedException(ExpectedException = typeof(SnippetExtractionException), ExpectedMessage = "Invalid extraction rule")]
        public void ExtractSnippetInvalidRule()
        {
            // Run the extraction
            new XmlSnippetExtractor(this.SourceDirectories).Extract(this.ComputeFilePath("Sample.xml"), "abc abc(abc");
        }

        /// <summary>
        /// Tests extract snippet with non matching member.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        [Test]
        [TestCase("Sample.xml", "//DoesntExist")]
        [ExpectedException(ExpectedException = typeof(SnippetExtractionException), ExpectedMessage = "Cannot find member")]
        public void ExtractSnippetNotFound(string fileName, string pattern)
        {
            // Resolve path
            fileName = this.ComputeFilePath(fileName);
            
            // Run the extraction
            XmlSnippetExtractor extractor = new XmlSnippetExtractor(this.SourceDirectories);
            Projbook.Core.Model.Snippet snippet = extractor.Extract(fileName, pattern);
        }
    }
}