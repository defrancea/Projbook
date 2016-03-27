using NUnit.Framework;
using Projbook.Extension.Exception;
using Projbook.Extension.Spi;
using Projbook.Extension.XmlExtractor;
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
                snippetExtractor = new XmlSnippetExtractor();
                this.extractorCache[fileName] = snippetExtractor;
            }
            Extension.Model.Snippet snippet = snippetExtractor.Extract(new StreamReader(this.LocateFile(fileName).OpenRead()), pattern);

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
            new XmlSnippetExtractor().Extract(new StreamReader(new FileInfo(Path.Combine(this.SourceDirectories[0].FullName, "Resources", "SourcesA", "Sample.xml")).OpenRead()), "abc abc(abc");
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
            XmlSnippetExtractor extractor = new XmlSnippetExtractor();
            Extension.Model.Snippet snippet = extractor.Extract(new StreamReader(new FileInfo(Path.Combine(this.SourceDirectories[0].FullName, fileName)).OpenRead()), pattern);
        }
    }
}