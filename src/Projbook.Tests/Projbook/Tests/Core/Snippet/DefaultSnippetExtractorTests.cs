using NUnit.Framework;
using Projbook.Core.Snippet;
using System;
using System.IO;

namespace Projbook.Tests.Core.Snippet
{
    /// <summary>
    /// Tests <see cref="DefaultSnippetExtractor"/>.
    /// </summary>
    [TestFixture]
    public class DefaultSnippetExtractorTests : AbstractTests
    {
        /// <summary>
        /// Tests extract snippet.
        /// </summary>
        [Test]
        public void ExtractSnippet()
        {
            // Run the extraction
            DefaultSnippetExtractor extractor = new DefaultSnippetExtractor();
            Projbook.Extension.Model.Snippet snippet = extractor.Extract(new StreamReader(new FileInfo(Path.Combine(this.SourceDirectories[0].FullName, "Resources", "Expected", "content.txt")).OpenRead()), null);

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