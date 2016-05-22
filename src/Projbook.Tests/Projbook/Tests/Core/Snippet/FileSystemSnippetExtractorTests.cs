using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using NUnit.Framework;
using Projbook.Extension.FileSystemExtractor;
using Projbook.Tests.Resources;

namespace Projbook.Tests.Core.Snippet
{
    /// <summary>
    /// Tests <see cref="FileSystemSnippetExtractor"/>.
    /// </summary>
    [TestFixture]
    public class FileSystemSnippetExtractorTests
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
                { "Source/AnyClass.cs", new MockFileData(SourceCSharpFiles.AnyClass) },
                { "Source/CSharp/Sample.cs", new MockFileData(SourceCSharpFiles.Sample) },
                { "Source/CSharp/NeedCleanup.cs", new MockFileData(SourceCSharpFiles.NeedCleanup) },
                { "Source/Empty.cs", new MockFileData(SourceCSharpFiles.Empty) },
                { "Source/Options.cs", new MockFileData(SourceCSharpFiles.Options) },
                

                { "Expected/AnyClass.txt", new MockFileData(ExpectedCSharpFiles.AnyClass) },
                { "Expected/NS.txt", new MockFileData(ExpectedCSharpFiles.NS) },
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
        /// <param name="directoryName">The source directory name.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="expectedFile">The expected file name.</param>
        [Test]
        [TestCase("Source", "", "")]
        public void ExtractSnippet(string directoryName, string pattern, string expectedFile)
        {
            // Run the extraction
            Extension.Model.Snippet snippet = new FileSystemSnippetExtractor().Extract(this.FileSystem.DirectoryInfo.FromDirectoryName(directoryName), pattern);

            // Assert
            Assert.IsTrue(snippet.Content.Length > 0); // TODO fails (include Source as well)
        }
    }
}
