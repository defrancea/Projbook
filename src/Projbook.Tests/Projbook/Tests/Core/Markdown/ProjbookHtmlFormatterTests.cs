using CommonMark;
using CommonMark.Syntax;
using NUnit.Framework;
using Projbook.Core.Markdown;
using Projbook.Extension.Model;
using System;
using System.IO;
using System.Text;

namespace Projbook.Tests.Core
{
    /// <summary>
    /// Tests <see cref="ProjbookHtmlFormatter"/>.
    /// </summary>
    [TestFixture]
    public class ProjbookHtmlFormatterTests
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
        public ProjbookHtmlFormatter Formatter { get; private set; }

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
            this.Formatter = new ProjbookHtmlFormatter("page", this.StreamWriter, CommonMarkSettings.Default, 0, new System.Collections.Generic.Dictionary<Guid, Extension.Model.Snippet>(), string.Empty);
        }

        /// <summary>
        /// Tests with invalid input.
        /// </summary>
        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void WrongInit(string contextName)
        {
            Assert.Throws(
                Is.TypeOf<ArgumentException>(),
                () => new ProjbookHtmlFormatter(contextName, this.StreamWriter, CommonMarkSettings.Default, 0, null, null));
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
            Assert.AreEqual(string.Format(@"<h0></h0>{0}", Environment.NewLine), output);
            Assert.AreEqual(1, this.Formatter.PageBreak.Length);
            Assert.AreEqual("page-unknown", this.Formatter.PageBreak[0].Id);
            Assert.AreEqual("unknown", this.Formatter.PageBreak[0].Title);
            Assert.AreEqual(0, this.Formatter.PageBreak[0].Position);
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
            Assert.AreEqual(string.Format(@"<h0>title</h0>{0}", Environment.NewLine), output);
            Assert.AreEqual(1, this.Formatter.PageBreak.Length);
            Assert.AreEqual("page-title", this.Formatter.PageBreak[0].Id);
            Assert.AreEqual("title", this.Formatter.PageBreak[0].Title);
            Assert.AreEqual(0, this.Formatter.PageBreak[0].Position);
        }

        /// <summary>
        /// Tests with simple header with specific title base.
        /// </summary>
        [Test]
        [TestCase]
        public void WriteSimpleHeaderDifferentBase()
        {
            // Reinit formatter
            this.Formatter = new ProjbookHtmlFormatter("page", this.StreamWriter, CommonMarkSettings.Default, 42, new System.Collections.Generic.Dictionary<Guid, Extension.Model.Snippet>(), string.Empty);

            // Process
            Block block = new Block(BlockTag.AtxHeader, 0);
            block.InlineContent = new Inline("title");
            string output = this.Process(block);

            // Assert
            Assert.AreEqual(string.Format(@"<h42>title</h42>{0}", Environment.NewLine), output);
            Assert.AreEqual(1, this.Formatter.PageBreak.Length);
            Assert.AreEqual("page-title", this.Formatter.PageBreak[0].Id);
            Assert.AreEqual("title", this.Formatter.PageBreak[0].Title);
            Assert.AreEqual(0, this.Formatter.PageBreak[0].Position);
        }

        /// <summary>
        /// Tests with simple header with pre content.
        /// </summary>
        [Test]
        [TestCase]
        public void WriteSimpleHeaderWithPreContent()
        {
            // Process
            Block block = new Block(BlockTag.Document, 0);
            block.InlineContent = new Inline("pre content");
            Block block1 = new Block(BlockTag.AtxHeader, 0);
            block1.InlineContent = new Inline("title");
            block.FirstChild = block1;
            string output = this.Process(block);

            // Assert
            Assert.AreEqual(string.Format(@"pre content{0}<h0>title</h0>{0}", Environment.NewLine), output);
            Assert.AreEqual(1, this.Formatter.PageBreak.Length);
            Assert.AreEqual("page-title", this.Formatter.PageBreak[0].Id);
            Assert.AreEqual("title", this.Formatter.PageBreak[0].Title);
            Assert.AreEqual(11, this.Formatter.PageBreak[0].Position);
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
            Assert.AreEqual(string.Format(@"<h42></h42>{0}", Environment.NewLine), output);
            Assert.AreEqual(1, this.Formatter.PageBreak.Length);
            Assert.AreEqual("page-unknown", this.Formatter.PageBreak[0].Id);
            Assert.AreEqual("unknown", this.Formatter.PageBreak[0].Title);
            Assert.AreEqual(0, this.Formatter.PageBreak[0].Position);
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
            Assert.AreEqual(string.Format(@"<h0>Title</h0>{0}", Environment.NewLine), output);
            Assert.AreEqual(1, this.Formatter.PageBreak.Length);
            Assert.AreEqual("page-title", this.Formatter.PageBreak[0].Id);
            Assert.AreEqual("Title", this.Formatter.PageBreak[0].Title);
            Assert.AreEqual(0, this.Formatter.PageBreak[0].Position);
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
            block.InlineContent = new Inline("This is a & super content en Français ?");
            string output = this.Process(block);

            // Assert
            Assert.AreEqual(string.Format(@"<h0>This is a &amp; super content en Français ?</h0>{0}", Environment.NewLine), output);
            Assert.AreEqual(1, this.Formatter.PageBreak.Length);
            Assert.AreEqual("page-this-is-a---super-content-en-fran-ais--", this.Formatter.PageBreak[0].Id);
            Assert.AreEqual("This is a & super content en Français ?", this.Formatter.PageBreak[0].Title);
            Assert.AreEqual(0, this.Formatter.PageBreak[0].Position);
        }

        /// <summary>
        /// Tests with conflicting title.
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
            Assert.AreEqual(string.Format(@"<h0>Title{0}<h0>Title</h0>{0}</h0>{0}", Environment.NewLine), output);
            Assert.AreEqual(2, this.Formatter.PageBreak.Length);
            Assert.AreEqual("page-title", this.Formatter.PageBreak[0].Id);
            Assert.AreEqual("Title", this.Formatter.PageBreak[0].Title);
            Assert.AreEqual(0, this.Formatter.PageBreak[0].Position);
            Assert.AreEqual("page-title-2", this.Formatter.PageBreak[1].Id);
            Assert.AreEqual("Title", this.Formatter.PageBreak[1].Title);
            Assert.AreEqual(9, this.Formatter.PageBreak[1].Position);
        }

        /// <summary>
        /// Tests with conflicting title with special char.
        /// </summary>
        [Test]
        [TestCase]
        public void WriteHeaderConflictSpecialChar()
        {
            // Process
            Block block1 = new Block(BlockTag.AtxHeader, 0);
            block1.InlineContent = new Inline("Title ?");
            Block block2 = new Block(BlockTag.AtxHeader, 0);
            block2.InlineContent = new Inline("Title !");
            Block block = new Block(BlockTag.Document, 0);
            block.FirstChild = block1;
            block.FirstChild.FirstChild = block2;
            string output = this.Process(block);

            // Assert
            Assert.AreEqual(string.Format(@"<h0>Title ?{0}<h0>Title !</h0>{0}</h0>{0}", Environment.NewLine), output);
            Assert.AreEqual(2, this.Formatter.PageBreak.Length);
            Assert.AreEqual("page-title--", this.Formatter.PageBreak[0].Id);
            Assert.AreEqual("Title ?", this.Formatter.PageBreak[0].Title);
            Assert.AreEqual(0, this.Formatter.PageBreak[0].Position);
            Assert.AreEqual("page-title---2", this.Formatter.PageBreak[1].Id);
            Assert.AreEqual("Title !", this.Formatter.PageBreak[1].Title);
            Assert.AreEqual(11, this.Formatter.PageBreak[1].Position);
        }

        /// <summary>
        /// Tests with conflicting title.
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
            Assert.AreEqual(string.Format(@"<h0>One/Title{0}<h0>One/Title</h0>{0}</h0>{0}", Environment.NewLine), output);
            Assert.AreEqual(2, this.Formatter.PageBreak.Length);
            Assert.AreEqual("page-one-title", this.Formatter.PageBreak[0].Id);
            Assert.AreEqual("One/Title", this.Formatter.PageBreak[0].Title);
            Assert.AreEqual(0, this.Formatter.PageBreak[0].Position);
            Assert.AreEqual("page-one-title-2", this.Formatter.PageBreak[1].Id);
            Assert.AreEqual("One/Title", this.Formatter.PageBreak[1].Title);
            Assert.AreEqual(13, this.Formatter.PageBreak[1].Position);
        }

        /// <summary>
        /// Tests with chained inline header.
        /// </summary>
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
            Assert.AreEqual(string.Format(@"<h0>Title in many siblings</h0>{0}", Environment.NewLine), output);
            Assert.AreEqual(1, this.Formatter.PageBreak.Length);
            Assert.AreEqual("page-title-in-many-siblings", this.Formatter.PageBreak[0].Id);
            Assert.AreEqual("Title in many siblings", this.Formatter.PageBreak[0].Title);
            Assert.AreEqual(0, this.Formatter.PageBreak[0].Position);
        }

        /// <summary>
        /// Tests with simple table.
        /// </summary>
        [Test]
        [TestCase]
        public void WriteSimpleTable()
        {
            // Process
            Block block = new Block(BlockTag.Paragraph, 0);
            block.InlineContent = new Inline("header 1 | header 2");
            block.InlineContent.NextSibling = new Inline("---- | ----");
            block.InlineContent.NextSibling.NextSibling = new Inline("value 1 | value 2");
            string output = this.Process(block);

            // Assert
            Assert.AreEqual(string.Format(@"<table class=""table""><tr><th class=""text-left"">header 1</th><th class=""text-left"">header 2</th></tr><tr><td class=""text-left"">value 1</td><td class=""text-left"">value 2</td></tr></table></p>{0}", Environment.NewLine), output);
        }

        /// <summary>
        /// Tests with simple table.
        /// </summary>
        [Test]
        [TestCase]
        public void WriteManyRowTable()
        {
            // Process
            Block block = new Block(BlockTag.Paragraph, 0);
            block.InlineContent = new Inline("header 1 | header 2");
            block.InlineContent.NextSibling = new Inline("---- | ----");
            block.InlineContent.NextSibling.NextSibling = new Inline("value 1 | value 2");
            block.InlineContent.NextSibling.NextSibling.NextSibling = new Inline("value 3 | value 4");
            block.InlineContent.NextSibling.NextSibling.NextSibling.NextSibling = new Inline("value 5 | value 6");
            string output = this.Process(block);

            // Assert
            Assert.AreEqual(string.Format(@"<table class=""table""><tr><th class=""text-left"">header 1</th><th class=""text-left"">header 2</th></tr><tr><td class=""text-left"">value 1</td><td class=""text-left"">value 2</td></tr><tr><td class=""text-left"">value 3</td><td class=""text-left"">value 4</td></tr><tr><td class=""text-left"">value 5</td><td class=""text-left"">value 6</td></tr></table></p>{0}", Environment.NewLine), output);
        }

        /// <summary>
        /// Tests with table with border.
        /// </summary>
        [Test]
        [TestCase]
        public void WriteBorderedTable()
        {
            // Process
            Block block = new Block(BlockTag.Paragraph, 0);
            block.InlineContent = new Inline("| header 1 | header 2 |");
            block.InlineContent.NextSibling = new Inline("| ---- | ---- |");
            block.InlineContent.NextSibling.NextSibling = new Inline("| value 1 | value 2 |");
            string output = this.Process(block);

            // Assert
            Assert.AreEqual(string.Format(@"<table class=""table""><tr><th class=""text-left"">header 1</th><th class=""text-left"">header 2</th></tr><tr><td class=""text-left"">value 1</td><td class=""text-left"">value 2</td></tr></table></p>{0}", Environment.NewLine), output);
        }

        /// <summary>
        /// Tests with table align.
        /// </summary>
        [Test]
        [TestCase]
        public void WriteTableAlign()
        {
            // Process
            Block block = new Block(BlockTag.Paragraph, 0);
            block.InlineContent = new Inline("| header 1 | header 2 | header 3 |");
            block.InlineContent.NextSibling = new Inline("| :--- | :--: | ---: |");
            block.InlineContent.NextSibling.NextSibling = new Inline("| value 1 | value 2 | value 3 |");
            string output = this.Process(block);

            // Assert
            Assert.AreEqual(string.Format(@"<table class=""table""><tr><th class=""text-left"">header 1</th><th class=""text-left"">header 2</th><th class=""text-left"">header 3</th></tr><tr><td class=""text-left"">value 1</td><td class=""text-center"">value 2</td><td class=""text-right"">value 3</td></tr></table></p>{0}", Environment.NewLine), output);
        }

        /// <summary>
        /// Tests table with partial delimited.
        /// </summary>
        [Test]
        [TestCase]
        public void WritePartialDelimiterTable()
        {
            // Process
            Block block = new Block(BlockTag.Paragraph, 0);
            block.InlineContent = new Inline("| header 1 | header 2 |");
            block.InlineContent.NextSibling = new Inline("| ----");
            block.InlineContent.NextSibling.NextSibling = new Inline("| value 1 | value 2 |");
            string output = this.Process(block);

            // Assert
            Assert.AreEqual(string.Format(@"<table class=""table""><tr><th class=""text-left"">header 1</th><th class=""text-left"">header 2</th></tr><tr><td class=""text-left"">value 1</td><td class=""text-left"">value 2</td></tr></table></p>{0}", Environment.NewLine), output);
        }

        /// <summary>
        /// Tests with table with one column.
        /// </summary>
        [Test]
        [TestCase]
        public void WriteOneColumnTable()
        {
            // Process
            Block block = new Block(BlockTag.Paragraph, 0);
            block.InlineContent = new Inline("| header 1");
            block.InlineContent.NextSibling = new Inline("| ----");
            block.InlineContent.NextSibling.NextSibling = new Inline("| value 1");
            string output = this.Process(block);

            // Assert
            Assert.AreEqual(string.Format(@"<table class=""table""><tr><th class=""text-left"">header 1</th></tr><tr><td class=""text-left"">value 1</td></tr></table></p>{0}", Environment.NewLine), output);
        }

        /// <summary>
        /// Tests with table that is not one.
        /// </summary>
        [Test]
        [TestCase]
        public void WritNoTable()
        {
            // Process
            Block block = new Block(BlockTag.Paragraph, 0);
            block.InlineContent = new Inline("header 1");
            block.InlineContent.NextSibling = new Inline("----");
            block.InlineContent.NextSibling.NextSibling = new Inline("value 1");
            string output = this.Process(block);

            // Assert
            Assert.AreEqual(string.Format(@"<p>header 1----value 1</p>{0}", Environment.NewLine), output);
        }

        /// <summary>
        /// Tests with table where the delimiter is at the wrong location.
        /// </summary>
        [Test]
        [TestCase]
        public void WritTableWrongDelimiterLocation()
        {
            // Process
            Block block = new Block(BlockTag.Paragraph, 0);
            block.InlineContent = new Inline("header 1");
            block.InlineContent.NextSibling = new Inline("value 1");
            block.InlineContent.NextSibling.NextSibling = new Inline("----");
            block.InlineContent.NextSibling.NextSibling.NextSibling = new Inline("value 2");
            string output = this.Process(block);

            // Assert
            Assert.AreEqual(string.Format(@"<p>header 1value 1----value 2</p>{0}", Environment.NewLine), output);
        }

        /// <summary>
        /// Tests with table with no delimiter.
        /// </summary>
        [Test]
        [TestCase]
        public void WritTableNoDelimiter()
        {
            // Process
            Block block = new Block(BlockTag.Paragraph, 0);
            block.InlineContent = new Inline("header 1");
            block.InlineContent.NextSibling = new Inline("value 1");
            block.InlineContent.NextSibling.NextSibling = new Inline("value 2");
            string output = this.Process(block);

            // Assert
            Assert.AreEqual(string.Format(@"<p>header 1value 1value 2</p>{0}", Environment.NewLine), output);
        }

        /// <summary>
        /// Tests table with a partially correct delimited.
        /// </summary>
        [Test]
        [TestCase]
        public void WritePartiallyCorrectDelimiterTable()
        {
            // Process
            Block block = new Block(BlockTag.Paragraph, 0);
            block.InlineContent = new Inline("| header 1 | header 2 |");
            block.InlineContent.NextSibling = new Inline("| ---- | foo |");
            block.InlineContent.NextSibling.NextSibling = new Inline("| value 1 | value 2 |");
            string output = this.Process(block);

            // Assert
            Assert.AreEqual(string.Format(@"<p>| header 1 | header 2 || ---- | foo || value 1 | value 2 |</p>{0}", Environment.NewLine), output);
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

        /// <summary>
        /// Test Node rendering as jsTree.
        /// </summary>
        [Test]
        public void RenderTree()
        {
            // Declare files
            Node file1 = new Node("File1.txt", true);
            Node file2 = new Node("File2.jpg", true);
            Node file3 = new Node("File3.cs", true);
            Node file4 = new Node("File4", true);

            // Declare folders
            Node root = new Node("Root", false);
            Node folder1 = new Node("Folder1", false);
            Node folder2 = new Node("Folder2", false);
            Node folder3 = new Node("Folder3", false);

            // Assign files to folders
            folder3.Children.Add(file1.Name, file1);
            folder3.Children.Add(file2.Name, file2);
            folder1.Children.Add(file3.Name, file3);

            // Assign folders to folders
            folder2.Children.Add(folder3.Name, folder3);
            root.Children.Add(folder1.Name, folder1);
            root.Children.Add(folder2.Name, folder2);
            root.Children.Add(file4.Name, file4);

            // Create snippet reference
            System.Collections.Generic.Dictionary<Guid, Extension.Model.Snippet> snippetReference = new System.Collections.Generic.Dictionary<Guid, Extension.Model.Snippet>();
            Guid guid = Guid.NewGuid();
            snippetReference[guid] = new NodeSnippet(root);

            // Define formatter
            this.Formatter = new ProjbookHtmlFormatter("page", this.StreamWriter, CommonMarkSettings.Default, 0, snippetReference, "prefix:");

            // Process
            Block block = new Block(BlockTag.HtmlBlock, 0);
            string content = "prefix:" + guid;
            block.StringContent = new StringContent();
            block.StringContent.Append(content, 0, content.Length);
            string output = this.Process(block);

            // Assert rendering
            Assert.AreEqual(
                @"<div class=""filetree""><ul><li data-jstree='{""type"":""folder""}'>Root<ul><li data-jstree='{""type"":""file""}'>File4</li></ul><ul><li data-jstree='{""type"":""folder""}'>Folder1<ul><li data-jstree='{""type"":""file""}'>File3.cs</li></ul></li></ul><ul><li data-jstree='{""type"":""folder""}'>Folder2<ul><li data-jstree='{""type"":""folder""}'>Folder3<ul><li data-jstree='{""type"":""file""}'>File1.txt</li></ul><ul><li data-jstree='{""type"":""file""}'>File2.jpg</li></ul></li></ul></li></ul></li></ul></div>",
                output);
        }
    }
}