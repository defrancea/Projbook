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
                new InjectAnchorHtmlFormatter("page", "UT", this.StreamWriter, CommonMarkSettings.Default);
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
            new InjectAnchorHtmlFormatter(contextName, "UT", this.StreamWriter, CommonMarkSettings.Default);
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
            Assert.AreEqual(@"<p></p>" + Environment.NewLine, output);
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
            Assert.AreEqual(string.Format(@"<!--UT [unknown](page.unknown)-->{0}<h0></h0>{0}", Environment.NewLine), output);
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
            Assert.AreEqual(string.Format(@"<!--UT [title](page.title)-->{0}<h0>title</h0>{0}", Environment.NewLine), output);
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
            Assert.AreEqual(string.Format(@"<!--UT [unknown](page.unknown)-->{0}<h42></h42>{0}", Environment.NewLine), output);
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
            Assert.AreEqual(string.Format(@"<!--UT [Title](page.title)-->{0}<h0>Title</h0>{0}", Environment.NewLine), output);
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
            Assert.AreEqual(string.Format(@"<!--UT [This is a & super content en Français](page.this+is+a+%26+super+content+en+fran%c3%a7ais)-->{0}<h0>This is a &amp; super content en Français</h0>{0}", Environment.NewLine), output);
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
            Assert.AreEqual(string.Format(@"<!--UT [Title](page.title)-->{0}<h0>Title<!--UT [Title](page.title-2)-->{0}<h0>Title</h0>{0}</h0>{0}", Environment.NewLine), output);
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
            Assert.AreEqual(string.Format(@"<!--UT [One/Title](page.one%2ftitle)-->{0}<h0>One/Title<!--UT [One/Title](page.one%2ftitle-2)-->{0}<h0>One/Title</h0>{0}</h0>{0}", Environment.NewLine), output);
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
            Assert.AreEqual(string.Format(@"<!--UT [Title in many siblings](page.title+in+many+siblings)-->{0}<h0>Title in many siblings</h0>{0}", Environment.NewLine), output);
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