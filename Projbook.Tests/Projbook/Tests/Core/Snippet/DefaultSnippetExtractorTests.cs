using NUnit.Framework;
using Projbook.Core.Exception;
using Projbook.Core.Snippet;
using System;
using System.IO;

namespace Projbook.Tests.Core.Snippet
{
    /// <summary>
    /// Tests <see cref="DefaultSnippetExtractor"/>.
    /// </summary>
    [TestFixture]
    public class DefaultSnippetExtractorTests : AbstractSnippetTests
    {
        /// <summary>
        /// Tests with invalid input.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WrongInitSourceDefaultDirectories()
        {
            new DefaultSnippetExtractor(null);
        }
        /// <summary>
        /// Tests with invalid input.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void WrongInitSourceEmptyDirectories()
        {
            new DefaultSnippetExtractor(new DirectoryInfo[0]);
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
            new DefaultSnippetExtractor(new DirectoryInfo("Foo")).Extract(filePath, string.Empty);
        }

        /// <summary>
        /// Tests extract snippet.
        /// </summary>
        [Test]
        public void ExtractSnippet()
        {
            // Run the extraction
            DefaultSnippetExtractor extractor = new DefaultSnippetExtractor(this.SourceDirectories);
            Projbook.Core.Model.Snippet snippet = extractor.Extract("content.txt", null);

            // Load the expected file content
            MemoryStream memoryStream = new MemoryStream();
            using (var fileReader = new StreamReader(new FileStream(Path.GetFullPath(Path.Combine("Resources", "Expected", "content.txt")), FileMode.Open)))
            using (var fileWriter = new StreamWriter(memoryStream))
            {
                fileWriter.Write(fileReader.ReadToEnd());
            }

            // Assert
            Assert.AreEqual(
                System.Text.Encoding.UTF8.GetString(memoryStream.ToArray()).Replace("\r\n", Environment.NewLine),
                snippet.Content.Replace("\r\n", Environment.NewLine));
        }
    }
}