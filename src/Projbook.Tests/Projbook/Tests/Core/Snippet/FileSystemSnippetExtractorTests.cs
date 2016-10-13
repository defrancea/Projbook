﻿using NUnit.Framework;
using Projbook.Extension.Exception;
using Projbook.Extension.FileSystemExtractor;
using Projbook.Extension.Model;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace Projbook.Tests.Core.Snippet
{
    /// <summary>
    /// Tests <see cref="DefaultSnippetExtractor"/>.
    /// </summary>
    [TestFixture]
    public class FileSystemSnippetExtractorTests
    {
        /// <summary>
        /// The file system.
        /// </summary>
        public IFileSystem fileSystem;

        /// <summary>
        /// Initializes the test.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Initialize file system
            this.fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "A/B/Content.jpg", new MockFileData(string.Empty) },
                { "A/Content.gif", new MockFileData(string.Empty) },
                { "Content.txt", new MockFileData(string.Empty) },
                { "Content2.txt", new MockFileData(string.Empty) },
                { "Foo/Content.txt", new MockFileData(string.Empty) },
                { "Foo/Content2.txt", new MockFileData(string.Empty) },
                { "Foo/Bar/Content2.txt", new MockFileData(string.Empty) },
                { "Foo/Bar/Foo/Bar/Content2.txt", new MockFileData(string.Empty) },
            });

        }

        /// <summary>
        /// Tests extract snippet with no filter.
        /// </summary>
        [Test]
        public void ExtractSnippetNoFilter()
        {
            // Run the extraction
            FileSystemSnippetExtractor extractor = new FileSystemSnippetExtractor();
            DirectoryInfoBase directoryInfoBase = fileSystem.DirectoryInfo.FromDirectoryName(fileSystem.Path.GetFullPath("Foo"));
            NodeSnippet snippet = extractor.Extract(directoryInfoBase, null) as NodeSnippet;

            // Assert Foo
            Assert.AreEqual("Foo", snippet.Node.Name);
            Assert.AreEqual(false, snippet.Node.IsLeaf);
            Assert.AreEqual(3, snippet.Node.Children.Count);

            // Assert Foo/Content.txt
            Node foocontent = snippet.Node.Children["Content.txt"];
            Assert.AreEqual("Content.txt", foocontent.Name);
            Assert.AreEqual(true, foocontent.IsLeaf);
            Assert.AreEqual(0, foocontent.Children.Count);

            // Assert Foo/Content2.txt
            Node foocontent2 = snippet.Node.Children["Content2.txt"];
            Assert.AreEqual("Content2.txt", foocontent2.Name);
            Assert.AreEqual(true, foocontent2.IsLeaf);
            Assert.AreEqual(0, foocontent2.Children.Count);

            // Assert Foo/Bar
            Node bar = snippet.Node.Children["Bar"];
            Assert.AreEqual("Bar", bar.Name);
            Assert.AreEqual(false, bar.IsLeaf);
            Assert.AreEqual(2, bar.Children.Count);

            // Assert Foo/Bar/Content2.txt
            Node barcontent2 = bar.Children["Content2.txt"];
            Assert.AreEqual("Content2.txt", barcontent2.Name);
            Assert.AreEqual(true, barcontent2.IsLeaf);
            Assert.AreEqual(0, barcontent2.Children.Count);

            // Assert Foo/Bar/Foo
            Node barfoo = bar.Children["Foo"];
            Assert.AreEqual("Foo", barfoo.Name);
            Assert.AreEqual(false, barfoo.IsLeaf);
            Assert.AreEqual(1, barfoo.Children.Count);

            // Assert Foo/Bar/Foo/Bar
            Node barfoobar = barfoo.Children["Bar"];
            Assert.AreEqual("Bar", barfoobar.Name);
            Assert.AreEqual(false, barfoobar.IsLeaf);
            Assert.AreEqual(1, barfoobar.Children.Count);

            // Assert Foo/Bar/Foo/Bar/Content2.txt
            Node barfoobarcontent2 = barfoobar.Children["Content2.txt"];
            Assert.AreEqual("Content2.txt", barfoobarcontent2.Name);
            Assert.AreEqual(true, barfoobarcontent2.IsLeaf);
            Assert.AreEqual(0, barfoobarcontent2.Children.Count);
        }

        /// <summary>
        /// Tests extract snippet with Content2.* filter.
        /// </summary>
        [Test]
        public void ExtractSnippetFilterContent2()
        {
            // Run the extraction
            FileSystemSnippetExtractor extractor = new FileSystemSnippetExtractor();
            DirectoryInfoBase directoryInfoBase = fileSystem.DirectoryInfo.FromDirectoryName(fileSystem.Path.GetFullPath("Foo"));
            NodeSnippet snippet = extractor.Extract(directoryInfoBase, "Content2.txt") as NodeSnippet;

            // Assert Foo
            Assert.AreEqual("Foo", snippet.Node.Name);
            Assert.AreEqual(false, snippet.Node.IsLeaf);
            Assert.AreEqual(2, snippet.Node.Children.Count);

            // Assert Foo/Content2.txt
            Node foocontent2 = snippet.Node.Children["Content2.txt"];
            Assert.AreEqual("Content2.txt", foocontent2.Name);
            Assert.AreEqual(true, foocontent2.IsLeaf);
            Assert.AreEqual(0, foocontent2.Children.Count);

            // Assert Foo/Bar
            Node bar = snippet.Node.Children["Bar"];
            Assert.AreEqual("Bar", bar.Name);
            Assert.AreEqual(false, bar.IsLeaf);
            Assert.AreEqual(2, bar.Children.Count);

            // Assert Foo/Bar/Content2.txt
            Node barcontent2 = bar.Children["Content2.txt"];
            Assert.AreEqual("Content2.txt", barcontent2.Name);
            Assert.AreEqual(true, barcontent2.IsLeaf);
            Assert.AreEqual(0, barcontent2.Children.Count);

            // Assert Foo/Bar/Foo
            Node barfoo = bar.Children["Foo"];
            Assert.AreEqual("Foo", barfoo.Name);
            Assert.AreEqual(false, barfoo.IsLeaf);
            Assert.AreEqual(1, barfoo.Children.Count);

            // Assert Foo/Bar/Foo/Bar
            Node barfoobar = barfoo.Children["Bar"];
            Assert.AreEqual("Bar", barfoobar.Name);
            Assert.AreEqual(false, barfoobar.IsLeaf);
            Assert.AreEqual(1, barfoobar.Children.Count);

            // Assert Foo/Bar/Foo/Bar/Content2.txt
            Node barfoobarcontent2 = barfoobar.Children["Content2.txt"];
            Assert.AreEqual("Content2.txt", barfoobarcontent2.Name);
            Assert.AreEqual(true, barfoobarcontent2.IsLeaf);
            Assert.AreEqual(0, barfoobarcontent2.Children.Count);
        }

        /// <summary>
        /// Tests extract snippet with multiple pattern.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        [Test]
        [TestCase("*.jpg|*.gif")]
        [TestCase("*.jpg;*.gif")]
        [TestCase("*.jpg|  ;|*.gif")]
        public void ExtractSnippetFilterMultiplePattern(string pattern)
        {
            // Run the extraction
            FileSystemSnippetExtractor extractor = new FileSystemSnippetExtractor();
            DirectoryInfoBase directoryInfoBase = fileSystem.DirectoryInfo.FromDirectoryName(fileSystem.Path.GetFullPath("."));
            NodeSnippet snippet = extractor.Extract(directoryInfoBase, pattern) as NodeSnippet;

            // Assert children number
            Assert.AreEqual(1, snippet.Node.Children.Count);
            
            // Assert A
            Node acontent = snippet.Node.Children["A"];
            Assert.AreEqual("A", acontent.Name);
            Assert.AreEqual(false, acontent.IsLeaf);
            Assert.AreEqual(2, acontent.Children.Count);

            // Assert A/Content.gif
            Node contentgif = acontent.Children["Content.gif"];
            Assert.AreEqual("Content.gif", contentgif.Name);
            Assert.AreEqual(true, contentgif.IsLeaf);
            Assert.AreEqual(0, contentgif.Children.Count);

            // Assert A/B
            Node bcontent = acontent.Children["B"];
            Assert.AreEqual("B", bcontent.Name);
            Assert.AreEqual(false, bcontent.IsLeaf);
            Assert.AreEqual(1, bcontent.Children.Count);

            // Assert A/B/Content.jpg
            Node contentjpg = bcontent.Children["Content.jpg"];
            Assert.AreEqual("Content.jpg", contentjpg.Name);
            Assert.AreEqual(true, contentjpg.IsLeaf);
            Assert.AreEqual(0, contentjpg.Children.Count);
        }

        /// <summary>
        /// Tests extract snippet with NotFound.txt filter.
        /// </summary>
        [Test]
        public void ExtractSnippetFilterNotFound()
        {
            // Run the extraction
            FileSystemSnippetExtractor extractor = new FileSystemSnippetExtractor();
            DirectoryInfoBase directoryInfoBase = fileSystem.DirectoryInfo.FromDirectoryName(fileSystem.Path.GetFullPath("Foo"));
            NodeSnippet snippet = extractor.Extract(directoryInfoBase, "NotFound.txt") as NodeSnippet;

            // Assert Foo
            Assert.AreEqual("Foo", snippet.Node.Name);
            Assert.AreEqual(false, snippet.Node.IsLeaf);
            Assert.AreEqual(0, snippet.Node.Children.Count);
        }

        /// <summary>
        /// Tests extract snippet with a wrong root.
        /// </summary>
        [Test]
        public void ExtractSnippetWrongRoot()
        {
            // Prepare the extraction
            FileSystemSnippetExtractor extractor = new FileSystemSnippetExtractor();
            DirectoryInfoBase directoryInfoBase = fileSystem.DirectoryInfo.FromDirectoryName("FooBar");

            // Run the extraction
            Assert.Throws(
                Is.TypeOf<SnippetExtractionException>().And.Message.EqualTo("Cannot find directory"),
                () => extractor.Extract(directoryInfoBase, null));
        }
    }
}