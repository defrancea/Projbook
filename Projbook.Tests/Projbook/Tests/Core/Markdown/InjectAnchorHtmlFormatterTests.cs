using CommonMark;
using CommonMark.Syntax;
using NUnit.Framework;
using Projbook.Core.Markdown;
using System;
using System.IO;
using System.Text;

namespace Projbook.Tests.Core
{
    /// <summary>
    /// Tests <see cref="InjectAnchorHtmlFormatter"/>.
    /// </summary>
    [TestFixture]
    public class InjectAnchorHtmlFormatterTests
    {
        /// <summary>
        /// The memory stream where the formatting is produced.
        /// </summary>
        public MemoryStream MemoryStream { get; private set; }

        /// <summary>
        /// The stream writter having as ouput the memory stream.
        /// </summary>
        public StreamWriter StreamWriter { get; private set; }

        /// <summary>
        /// The tested formatter.
        /// </summary>
        public InjectAnchorHtmlFormatter Formatter { get; private set; }

        /// <summary>
        /// Initializes the test with fresh memory stream and formatter.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Initialize ouput
            this.MemoryStream = new MemoryStream();
            this.StreamWriter = new StreamWriter(this.MemoryStream);
            
            // Initialize formatter
            this.Formatter =
                new InjectAnchorHtmlFormatter("page", this.StreamWriter, CommonMarkSettings.Default);
        }

        /// <summary>
        /// Tests with invalid input.
        /// </summary>
        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [ExpectedException(typeof(ArgumentException))]
        public void WrongInit(string contextName)
        {
            new InjectAnchorHtmlFormatter(contextName, this.StreamWriter, CommonMarkSettings.Default);
        }

        /// <summary>
        /// Tests without header block.
        /// </summary>
        [Test]
        [TestCase]
        public void WriteNonHeader()
        {
            // Process
            Block block = new Block(BlockTag.Paragraph, 0);
            string output = this.Process(block);

            // Assert
            Assert.IsFalse(output.StartsWith(@"<a class=""anchor"" name="""));
            Assert.AreEqual(0, this.Formatter.Anchors.Length);
        }

        /// <summary>
        /// Tests with empty header.
        /// </summary>
        [Test]
        [TestCase]
        public void WriteEmptyHeader()
        {
            // Process
            Block block = new Block(BlockTag.AtxHeader, 0);
            string output = this.Process(block);

            // Assert
            Assert.IsTrue(output.StartsWith(@"<a class=""anchor"" name=""page-unknown"">"));
            Assert.AreEqual(1, this.Formatter.Anchors.Length);
            Assert.AreEqual("page-unknown", this.Formatter.Anchors[0].Value);
            Assert.AreEqual("unknown", this.Formatter.Anchors[0].Label);
            Assert.AreEqual(0, this.Formatter.Anchors[0].Level);
        }

        /// <summary>
        /// Tests with simple header.
        /// </summary>
        [Test]
        [TestCase]
        public void WriteSimpleHeader()
        {
            // Process
            Block block = new Block(BlockTag.AtxHeader, 0);
            block.InlineContent = new Inline("title");
            string output = this.Process(block);

            // Assert
            Assert.IsTrue(output.StartsWith(@"<a class=""anchor"" name=""page-title"">"));
            Assert.AreEqual(1, this.Formatter.Anchors.Length);
            Assert.AreEqual("page-title", this.Formatter.Anchors[0].Value);
            Assert.AreEqual("title", this.Formatter.Anchors[0].Label);
            Assert.AreEqual(0, this.Formatter.Anchors[0].Level);
        }

        /// <summary>
        /// Tests with simple with level.
        /// </summary>
        [Test]
        [TestCase]
        public void WriteSimpleHeaderLevel()
        {
            // Process
            Block block = new Block(BlockTag.AtxHeader, 0);
            block.HeaderLevel = 42;
            string output = this.Process(block);

            // Assert
            Assert.IsTrue(output.StartsWith(@"<a class=""anchor"" name=""page-unknown"">"));
            Assert.AreEqual(1, this.Formatter.Anchors.Length);
            Assert.AreEqual("page-unknown", this.Formatter.Anchors[0].Value);
            Assert.AreEqual("unknown", this.Formatter.Anchors[0].Label);
            Assert.AreEqual(42, this.Formatter.Anchors[0].Level);
        }

        /// <summary>
        /// Tests with upper case char.
        /// </summary>
        [Test]
        [TestCase]
        public void WriteHeaderToLower()
        {
            // Process
            Block block = new Block(BlockTag.AtxHeader, 0);
            block.InlineContent = new Inline("Title");
            string output = this.Process(block);

            // Assert
            Assert.IsTrue(output.StartsWith(@"<a class=""anchor"" name=""page-title"">"));
            Assert.AreEqual(1, this.Formatter.Anchors.Length);
            Assert.AreEqual("page-title", this.Formatter.Anchors[0].Value);
            Assert.AreEqual("Title", this.Formatter.Anchors[0].Label);
            Assert.AreEqual(0, this.Formatter.Anchors[0].Level);
        }

        /// <summary>
        /// Tests with url non compatible char.
        /// </summary>
        [Test]
        [TestCase]
        public void WriteHeaderEncode()
        {
            // Process
            Block block = new Block(BlockTag.AtxHeader, 0);
            block.InlineContent = new Inline("This is a & super content en Français");
            string output = this.Process(block);

            // Assert
            Assert.IsTrue(output.StartsWith(@"<a class=""anchor"" name=""page-this+is+a+%26+super+content+en+fran%c3%a7ais"">"));
            Assert.AreEqual(1, this.Formatter.Anchors.Length);
            Assert.AreEqual("page-this+is+a+%26+super+content+en+fran%c3%a7ais", this.Formatter.Anchors[0].Value);
            Assert.AreEqual("This is a & super content en Français", this.Formatter.Anchors[0].Label);
            Assert.AreEqual(0, this.Formatter.Anchors[0].Level);
        }

        /// <summary>
        /// Tests with conflicting anchor.
        /// </summary>
        [Test]
        [TestCase]
        public void WriteHeaderConflict()
        {
            // Process
            Block block1 = new Block(BlockTag.AtxHeader, 0);
            block1.InlineContent = new Inline("Title");
            Block block2 = new Block(BlockTag.AtxHeader, 0);
            block2.InlineContent = new Inline("Title");
            Block block = new Block(BlockTag.Document, 0);
            block.FirstChild = block1;
            block.FirstChild.FirstChild = block2;
            string output = this.Process(block);

            // Assert
            Assert.IsTrue(output.Contains(@"<a class=""anchor"" name=""page-title"">"));
            Assert.IsTrue(output.Contains(@"<a class=""anchor"" name=""page-title-2"">"));
            Assert.AreEqual(2, this.Formatter.Anchors.Length);
            Assert.AreEqual("page-title", this.Formatter.Anchors[0].Value);
            Assert.AreEqual("Title", this.Formatter.Anchors[0].Label);
            Assert.AreEqual(0, this.Formatter.Anchors[0].Level);
            Assert.AreEqual("page-title-2", this.Formatter.Anchors[1].Value);
            Assert.AreEqual("Title", this.Formatter.Anchors[1].Label);
            Assert.AreEqual(0, this.Formatter.Anchors[1].Level);
        }

        /// <summary>
        /// Tests with conflicting anchor.
        /// </summary>
        [Test]
        [TestCase]
        public void WriteHeaderConflictWithEncodedChar()
        {
            // Process
            Block block1 = new Block(BlockTag.AtxHeader, 0);
            block1.InlineContent = new Inline("One/Title");
            Block block2 = new Block(BlockTag.AtxHeader, 0);
            block2.InlineContent = new Inline("One/Title");
            Block block = new Block(BlockTag.Document, 0);
            block.FirstChild = block1;
            block.FirstChild.FirstChild = block2;
            string output = this.Process(block);

            // Assert
            Assert.IsTrue(output.Contains(@"<a class=""anchor"" name=""page-one%2ftitle"">"));
            Assert.IsTrue(output.Contains(@"<a class=""anchor"" name=""page-one%2ftitle-2"">"));
            Assert.AreEqual(2, this.Formatter.Anchors.Length);
            Assert.AreEqual("page-one%2ftitle", this.Formatter.Anchors[0].Value);
            Assert.AreEqual("One/Title", this.Formatter.Anchors[0].Label);
            Assert.AreEqual(0, this.Formatter.Anchors[0].Level);
            Assert.AreEqual("page-one%2ftitle-2", this.Formatter.Anchors[1].Value);
            Assert.AreEqual("One/Title", this.Formatter.Anchors[1].Label);
            Assert.AreEqual(0, this.Formatter.Anchors[1].Level);
        }

        // Tests with chained inline header.
        [Test]
        [TestCase]
        public void WriteChainedInlineHeader()
        {
            // Process
            Block block = new Block(BlockTag.AtxHeader, 0);
            block.InlineContent = new Inline("Title");
            block.InlineContent.NextSibling = new Inline(" in many");
            block.InlineContent.NextSibling.NextSibling = new Inline(" siblings");
            string output = this.Process(block);

            // Assert
            Assert.IsTrue(output.StartsWith(@"<a class=""anchor"" name=""page-title+in+many+siblings"">"));
            Assert.AreEqual(1, this.Formatter.Anchors.Length);
            Assert.AreEqual("page-title+in+many+siblings", this.Formatter.Anchors[0].Value);
            Assert.AreEqual("Title in many siblings", this.Formatter.Anchors[0].Label);
            Assert.AreEqual(0, this.Formatter.Anchors[0].Level);
        }

        /// <summary>
        /// Processes the given block.
        /// </summary>
        /// <param name="block">The block to process.</param>
        /// <returns>The produced output.</returns>
        private string Process(Block block)
        {
            // Process
            this.Formatter.WriteDocument(block);
            this.StreamWriter.Flush();

            // Return output
            return Encoding.UTF8.GetString(this.MemoryStream.ToArray());
        }
    }
}