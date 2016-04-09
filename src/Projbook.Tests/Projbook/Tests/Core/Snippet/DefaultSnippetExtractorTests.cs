using NUnit.Framework;
using Projbook.Extension;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace Projbook.Tests.Core.Snippet
{
    /// <summary>
    /// Tests <see cref="DefaultSnippetExtractor"/>.
    /// </summary>
    [TestFixture]
    public class DefaultSnippetExtractorTests
    {
        /// <summary>
        /// Tests extract snippet.
        /// </summary>
        [Test]
        public void ExtractSnippet()
        {
            // Declare content
            const string content =
@"Some
Content
Here";
            // Mock file system
            IFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "Content.txt", new MockFileData(content) }
            });

            // Run the extraction
            DefaultSnippetExtractor extractor = new DefaultSnippetExtractor();
            FileInfoBase fileInfoBase = fileSystem.FileInfo.FromFileName("Content.txt");
            Extension.Model.Snippet snippet = extractor.Extract(fileInfoBase, null);

            // Load the expected file content
            MemoryStream memoryStream = new MemoryStream();
            using (var fileReader = new StreamReader(fileInfoBase.OpenRead()))
            using (var fileWriter = new StreamWriter(memoryStream))
            {
                fileWriter.Write(fileReader.ReadToEnd());
            }
            
            // Assert
            Assert.AreEqual(content, snippet.Content);
        }
    }
}