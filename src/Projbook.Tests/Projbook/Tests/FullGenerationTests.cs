using NUnit.Framework;
using Projbook.Core;
using Projbook.Core.Model;
using Projbook.Core.Model.Configuration;
using Projbook.Tests.Resources;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;

namespace Projbook.Tests.Core
{
    /// <summary>
    /// Perform a full generation with a testing template.
    /// </summary>
    [TestFixture]
    public class FullGenerationTests
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
                { "Project.csproj", new MockFileData("<Root></Root>") },
                { "Config/AllValues.json", new MockFileData(ConfigFiles.AllValues) },
                { "Config/NoPdf.json", new MockFileData(ConfigFiles.NoPdf) },
                { "Config/NoHtml.json", new MockFileData(ConfigFiles.NoHtml) },
                { "Config/ErrorInHtml.json", new MockFileData(ConfigFiles.ErrorInHtml) },
                { "Config/ErrorInHtmlNoPdf.json", new MockFileData(ConfigFiles.ErrorInHtmlNoPdf) },
                { "Config/MissingMembersInPage.json", new MockFileData(ConfigFiles.MissingMembersInPage) },
                { "Template/Template.txt", new MockFileData(TemplateFiles.Simple) },
                { "Template/Template-pdf.txt", new MockFileData(TemplateFiles.Simple_pdf) },
                { "Template/Malformated.txt", new MockFileData(TemplateFiles.Malformated) },
                { "Page/Content.md", new MockFileData(PageFiles.Content) },
                { "Page/Snippet.md", new MockFileData(PageFiles.Snippet) },
                { "Page/Table.md", new MockFileData(PageFiles.Table) },
                { "Page/MissingMembers.md", new MockFileData(PageFiles.MissingMembers) },
                { "Source/Foo.cs", new MockFileData(SourceCSharpFiles.Foo) },
                { "Expected/Simple.txt", new MockFileData(ExpectedFullGenerationFiles.Simple) },
                { "Expected/Simple-pdf.txt", new MockFileData(ExpectedFullGenerationFiles.Simple_pdf) },
            });
        }

        /// <summary>
        /// Run the full generation and compate the generated output with the expected output.
        /// </summary>
        /// <param name="configFileName">The config file name.</param>
        /// <param name="expectedHtmlFileName">The file name containing the expected content for HTML generation.</param>
        /// <param name="expectedPdfFileName">The file name containing the expected content for PDF generation.</param>
        /// <param name="generatedHtmlFileName">The file name containing the generated content for HTML.</param>
        /// <param name="generatedPdfFileName">The file name containing the generated content for PDF.</param>
        /// <param name="readOnly">Make files read only.</param>
        [Test]
        [TestCase("Config/AllValues.json", "Expected/Simple.txt", "Expected/Simple-pdf.txt", "doc.html", "doc-pdf-input.html", false)]
        [TestCase("Config/AllValues.json", "Expected/Simple.txt", "Expected/Simple-pdf.txt", "doc.html", "doc-pdf-input.html", true)]
        [TestCase("Config/NoPdf.json", "Expected/Simple.txt", "", "Template-generated.txt", "Template-pdf-generation.txt", false)]
        [TestCase("Config/NoHtml.json", "", "Expected/Simple-pdf.txt", "Template-generated.txt", "Template-pdf-generated.txt", false)]
        public void FullGeneration(string configFileName, string expectedHtmlFileName, string expectedPdfFileName, string generatedHtmlFileName, string generatedPdfFileName, bool readOnly)
        {
            // Prepare configuration
            Configuration configuration = new ConfigurationLoader(this.FileSystem).Load(".", configFileName)[0];
            string htmlTemplatePath = configuration.TemplateHtml;
            string pdfTemplatePath = configuration.TemplatePdf;
            string firstPagePath = new FileInfo(configuration.Pages.First().Path).FullName;

            // Retrieve attribute of html template
            FileAttributes? htmlTemplateFileAttributes = null;
            if (this.FileSystem.File.Exists(htmlTemplatePath))
            {
                htmlTemplateFileAttributes = this.FileSystem.File.GetAttributes(htmlTemplatePath);
            }

            // Retrieve attribute of pdf template
            FileAttributes? pdfTemplateFileAttributes = null;
            if (this.FileSystem.File.Exists(pdfTemplatePath))
            {
                pdfTemplateFileAttributes = this.FileSystem.File.GetAttributes(pdfTemplatePath);
            }

            // Retrieve attribute of the first markdown page
            FileAttributes? firstPageFileAttributes = null;
            if (this.FileSystem.File.Exists(firstPagePath))
            {
                firstPageFileAttributes = this.FileSystem.File.GetAttributes(firstPagePath);
            }

            // Make the file read only
            if (readOnly)
            {
                if (null != htmlTemplateFileAttributes)
                {
                    this.FileSystem.File.SetAttributes(htmlTemplatePath, htmlTemplateFileAttributes.Value | FileAttributes.ReadOnly);
                }
                if (null != pdfTemplateFileAttributes)
                {
                    this.FileSystem.File.SetAttributes(pdfTemplatePath, pdfTemplateFileAttributes.Value | FileAttributes.ReadOnly);
                }
                if (null != firstPageFileAttributes)
                {
                    this.FileSystem.File.SetAttributes(firstPagePath, firstPageFileAttributes.Value | FileAttributes.ReadOnly);
                }
            }

            // Execute test
            try
            {
                // Execute generation
                GenerationError[] errors = new ProjbookEngine(this.FileSystem, "Project.csproj", configuration, ".").Generate();

                // Read expected ouput
                string expectedContent = string.IsNullOrWhiteSpace(expectedHtmlFileName) ? string.Empty : this.FileSystem.File.ReadAllText(expectedHtmlFileName);

                // Read expected ouput
                string expectedPdfContent = string.IsNullOrWhiteSpace(expectedPdfFileName) ? string.Empty : this.FileSystem.File.ReadAllText(expectedPdfFileName);

                // Read generated ouput
                string generatedContent = !this.FileSystem.File.Exists(generatedHtmlFileName) ? string.Empty : this.FileSystem.File.ReadAllText(generatedHtmlFileName);

                // Read generated pdf ouput
                string generatedPdfContent = !this.FileSystem.File.Exists(generatedPdfFileName) ? string.Empty : this.FileSystem.File.ReadAllText(generatedPdfFileName);

                // Remove line return for cross platform platform testing
                expectedContent = expectedContent.Replace("\r", string.Empty).Replace("\n", string.Empty);
                expectedPdfContent = expectedPdfContent.Replace("\r", string.Empty).Replace("\n", string.Empty);
                generatedContent = generatedContent.Replace("\r", string.Empty).Replace("\n", string.Empty);
                generatedPdfContent = generatedPdfContent.Replace("\r", string.Empty).Replace("\n", string.Empty);
                
                // Assert result
                Assert.IsNotNull(errors);
                Assert.AreEqual(expectedContent, generatedContent);
                Assert.AreEqual(expectedPdfContent, generatedPdfContent);

                // PDF generation contains error becasue wkhtmltopdf doesn't abstract the file system
                // The generated input html file for pdf generation is then missing and raise an error during unit tests
                // Assert.AreEqual(0, errors.Length);

#if !NOPDF
                Assert.AreEqual(configuration.GeneratePdf, this.FileSystem.File.Exists(this.FileSystem.Path.ChangeExtension(configuration.OutputPdf, "pdf")));
#endif
            }
            finally
            {
                if (readOnly)
                {
                    if (null != htmlTemplateFileAttributes)
                    {
                        this.FileSystem.File.SetAttributes(htmlTemplatePath, htmlTemplateFileAttributes.Value);
                    }
                    if (null != pdfTemplateFileAttributes)
                    {
                        this.FileSystem.File.SetAttributes(pdfTemplatePath, pdfTemplateFileAttributes.Value);
                    }
                    if (null != firstPageFileAttributes)
                    {
                        this.FileSystem.File.SetAttributes(firstPagePath, firstPageFileAttributes.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Run the full generation with invalid template.
        /// </summary>
        [Test]
        public void FullGenerationErrorTemplate()
        {
            // Perform generation
            Configuration configuration = new ConfigurationLoader(this.FileSystem).Load(".", "Config/ErrorInHtml.json")[0];
            GenerationError[] errors = new ProjbookEngine(this.FileSystem, "Project.csproj", configuration, ".").Generate();

            // Assert result
            Assert.IsNotNull(errors);
            Assert.AreEqual(2, errors.Length);
            Assert.IsTrue(errors[0].SourceFile.EndsWith("Template/Malformated.txt"));
            Assert.IsTrue(errors[1].SourceFile.EndsWith("Template/Malformated.txt"));
            Assert.AreEqual(@"Error during HTML generation: (16:1) - Encountered end tag ""Sections"" with no matching start tag.  Are your start/end tags properly balanced?", errors[0].Message);
            Assert.AreEqual(16, errors[0].Line);
            Assert.AreEqual(1, errors[0].Column);
            Assert.AreEqual(@"Error during PDF generation: (16:1) - Encountered end tag ""Sections"" with no matching start tag.  Are your start/end tags properly balanced?", errors[1].Message);
            Assert.AreEqual(16, errors[1].Line);
            Assert.AreEqual(1, errors[1].Column);
        }

        /// <summary>
        /// Run the full generation with unexistingMembers.
        /// </summary>
        [Test]
        [TestCase]
        public void FullGenerationUnexistingMembers()
        {
            // Perform generation
            Configuration configuration = new ConfigurationLoader(this.FileSystem).Load(".", "Config/MissingMembersInPage.json")[0];
            GenerationError[] errors = new ProjbookEngine(this.FileSystem, "Project.csproj", configuration, ".").Generate();

            // Assert result
            Assert.IsNotNull(errors);
            Assert.AreEqual(3, errors.Length);
            Assert.AreEqual("Cannot find member: NotFound", errors[0].Message);
            Assert.AreEqual(1, errors[0].Line);
            Assert.AreEqual(27, errors[0].Column);
            Assert.AreEqual("Cannot find member: NotFound", errors[1].Message);
            Assert.AreEqual(7, errors[1].Line);
            Assert.AreEqual(27, errors[1].Column);
            Assert.IsTrue(errors[2].Message.StartsWith("Cannot find target 'Source/NotFound.cs' in any referenced project"));
            Assert.AreEqual(13, errors[2].Line);
            Assert.AreEqual(12, errors[2].Column);
        }
    }
}