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
        [ExpectedException(typeof(ArgumentException))]
        public void WrongInitSourceDefaultDirectories()
        {
            new DefaultSnippetExtractor("txt", "content.txt");
        }
        /// <summary>
        /// Tests with invalid input.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void WrongInitSourceEmptyDirectories()
        {
            new DefaultSnippetExtractor("txt", "content.txt", new DirectoryInfo[0]);
        }

        /// <summary>
        /// Tests with invalid input.
        /// </summary>
        [Test]
        [ExpectedException(typeof(SnippetExtractionException))]
        public void WrongInitEmpty()
        {
            new DefaultSnippetExtractor("txt", null, new DirectoryInfo[] { new DirectoryInfo("Foo") });
            new DefaultSnippetExtractor("txt", string.Empty, new DirectoryInfo[] { new DirectoryInfo("Foo") });
            new DefaultSnippetExtractor("txt", "   ", new DirectoryInfo[] { new DirectoryInfo("Foo") });
        }

        /// <summary>
        /// Tests extract snippet.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="expectedFile">The expected file.</param>
        [Test]
        [TestCase("txt", "content.txt", "content.txt")]
        [TestCase("", "content.txt", "content.txt")]
        [TestCase(null, "content.txt", "content.txt")]
        public void ExtractSnippet(string language, string pattern, string expectedFile)
        {
            // Run the extraction
            DefaultSnippetExtractor extractor = new DefaultSnippetExtractor("txt", pattern, this.SourceDirectories);
            Projbook.Core.Model.Snippet snippet = extractor.Extract();

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
    }
}