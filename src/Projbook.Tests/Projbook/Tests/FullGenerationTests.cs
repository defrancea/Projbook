using NUnit.Framework;
using Projbook.Core;
using Projbook.Core.Model;
using Projbook.Core.Model.Configuration;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace Projbook.Tests.Core
{
    /// <summary>
    /// Perform a full generation with a testing template.
    /// </summary>
    [TestFixture]
    public class FullGenerationTests : AbstractTests
    {
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
        [TestCase("testConfigAllValues.json", "expected.txt", "expected-pdf.txt", "doc.html", "doc-pdf-input.html", false)]
        [TestCase("testConfigAllValues.json", "expected.txt", "expected-pdf.txt", "doc.html", "doc-pdf-input.html", true)]
        [TestCase("testConfigNoPdf.json", "expected.txt", "", "testTemplate-generated.txt", "testTemplate-pdf-generated.txt", false)]
        [TestCase("testConfigNoHtml.json", "", "expected-pdf.txt", "testTemplate-generated.txt", "testTemplate-pdf-generated.txt", false)]
        public void FullGeneration(string configFileName, string expectedHtmlFileName, string expectedPdfFileName, string generatedHtmlFileName, string generatedPdfFileName, bool readOnly)
        {
            // Ensure nothing generated yet
            this.EnsureNoFile(generatedHtmlFileName);
            this.EnsureNoFile(generatedPdfFileName);

            // Perform generation
            Configuration configuration = new ConfigurationLoader(new FileSystem()).Load(this.SourceDirectories[0].FullName, "Resources/" + configFileName)[0];
            string htmlTemplatePath = configuration.TemplateHtml;
            string pdfTemplatePath = configuration.TemplatePdf;
            string firstPagePath = new FileInfo(configuration.Pages.First().Path).FullName;

            // Retrieve attribute of html template
            FileAttributes? htmlTemplateFileAttributes = null;
            if (File.Exists(htmlTemplatePath))
            {
                htmlTemplateFileAttributes = File.GetAttributes(htmlTemplatePath);
            }

            // Retrieve attribute of pdf template
            FileAttributes? pdfTemplateFileAttributes = null;
            if (File.Exists(pdfTemplatePath))
            {
                pdfTemplateFileAttributes = File.GetAttributes(pdfTemplatePath);
            }

            // Retrieve attribute of the first markdown page
            FileAttributes? firstPageFileAttributes = null;
            if (File.Exists(firstPagePath))
            {
                firstPageFileAttributes = File.GetAttributes(firstPagePath);
            }

            // Make the file read only
            if (readOnly)
            {
                if (null != htmlTemplateFileAttributes)
                {
                    File.SetAttributes(htmlTemplatePath, htmlTemplateFileAttributes.Value | FileAttributes.ReadOnly);
                }
                if (null != pdfTemplateFileAttributes)
                {
                    File.SetAttributes(pdfTemplatePath, pdfTemplateFileAttributes.Value | FileAttributes.ReadOnly);
                }
                if (null != firstPageFileAttributes)
                {
                    File.SetAttributes(firstPagePath, firstPageFileAttributes.Value | FileAttributes.ReadOnly);
                }
            }

            // Execute test
            try
            {
                // Execute generation
                GenerationError[] errors = new ProjbookEngine(new FileSystem(), "../../Projbook.Tests.csproj", configuration, ".").Generate();

                // Read expected ouput
                string expectedContent = this.LoadFile("Resources/FullGeneration/" + expectedHtmlFileName);

                // Read expected ouput
                string expectedPdfContent = this.LoadFile("Resources/FullGeneration/" + expectedPdfFileName);

                // Read generated ouput
                string generatedContent = this.LoadFile(generatedHtmlFileName);

                // Read generated pdf ouput
                string generatedPdfContent = this.LoadFile(generatedPdfFileName);

                // Remove line return for cross platform platform testing
                expectedContent = expectedContent.Replace("\r", string.Empty).Replace("\n", string.Empty);
                expectedPdfContent = expectedPdfContent.Replace("\r", string.Empty).Replace("\n", string.Empty);
                generatedContent = generatedContent.Replace("\r", string.Empty).Replace("\n", string.Empty);
                generatedPdfContent = generatedPdfContent.Replace("\r", string.Empty).Replace("\n", string.Empty);
                
                // Assert result
                Assert.IsNotNull(errors);
                Assert.AreEqual(0, errors.Length);
                Assert.AreEqual(expectedContent, generatedContent);
                Assert.AreEqual(expectedPdfContent, generatedPdfContent);

#if !NOPDF
                Assert.AreEqual(configuration.GeneratePdf, File.Exists(Path.ChangeExtension(configuration.OutputPdf, "pdf")));
#endif
            }
            finally
            {
                if (readOnly)
                {
                    if (null != htmlTemplateFileAttributes)
                    {
                        File.SetAttributes(htmlTemplatePath, htmlTemplateFileAttributes.Value);
                    }
                    if (null != pdfTemplateFileAttributes)
                    {
                        File.SetAttributes(pdfTemplatePath, pdfTemplateFileAttributes.Value);
                    }
                    if (null != firstPageFileAttributes)
                    {
                        File.SetAttributes(firstPagePath, firstPageFileAttributes.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Run the full generation with invalid template.
        /// </summary>
        /// <param name="configFileName">The config file name.</param>
        /// <param name="firstErrorFile">The file where the first error occured.</param>
        /// <param name="secondErrorFile">The file where the second error occured.</param>
        [Test]
        [TestCase("testConfigErrorInHtml.json", "testTemplateError.txt", "testTemplateError-pdf.txt")]
        [TestCase("testConfigErrorInHtmlNoPdf.json", "testTemplateErrorNoPdf.txt", "testTemplateErrorNoPdf.txt")]
        public void FullGenerationErrorTemplate(string configFileName, string firstErrorFile, string secondErrorFile)
        {
            // Perform generation
            Configuration configuration = new ConfigurationLoader(new FileSystem()).Load(this.SourceDirectories[0].FullName,"Resources/" + configFileName)[0];
            GenerationError[] errors = new ProjbookEngine(new FileSystem(), "../../Projbook.Tests.csproj", configuration, ".").Generate();

            // Assert result
            Assert.IsNotNull(errors);
            Assert.AreEqual(2, errors.Length);
            Assert.IsTrue(errors[0].SourceFile.EndsWith(firstErrorFile));
            Assert.IsTrue(errors[1].SourceFile.EndsWith(secondErrorFile));
            Assert.AreEqual(@"Error during HTML generation: (16:1) - Encountered end tag ""Sections"" with no matching start tag.  Are your start/end tags properly balanced?", errors[0].Message);
            Assert.AreEqual(16, errors[0].Line);
            Assert.AreEqual(1, errors[0].Column);
            Assert.AreEqual(@"Error during PDF generation: (16:1) - Encountered end tag ""Sections"" with no matching start tag.  Are your start/end tags properly balanced?", errors[1].Message);
            Assert.AreEqual(16, errors[1].Line);
            Assert.AreEqual(1, errors[1].Column);
        }

        /// <summary>
        /// Run the full generation with invalid template.
        /// </summary>
        [Test]
        [TestCase]
        public void FullGenerationErrorInPdfTemplate()
        {
            // Perform generation
            Configuration configuration = new ConfigurationLoader(new FileSystem()).Load(this.SourceDirectories[0].FullName, "Resources/testConfigErrorInPdf.json")[0];
            GenerationError[] errors = new ProjbookEngine(new FileSystem(), "../../Projbook.Tests.csproj", configuration, ".").Generate();

            // Assert result
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Length);
            Assert.IsTrue(errors[0].SourceFile.EndsWith("testTemplateErrorInPdf-pdf.txt"));
            Assert.AreEqual(@"Error during PDF generation: (16:1) - Encountered end tag ""Sections"" with no matching start tag.  Are your start/end tags properly balanced?", errors[0].Message);
            Assert.AreEqual(16, errors[0].Line);
            Assert.AreEqual(1, errors[0].Column);

        }

        /// <summary>
        /// Run the full generation with unexistingMembers.
        /// </summary>
        [Test]
        [TestCase]
        public void FullGenerationUnexistingMembers()
        {
            // Perform generation
            Configuration configuration = new ConfigurationLoader(new FileSystem()).Load(this.SourceDirectories[0].FullName, "Resources/testConfigUnexistingMembers.json")[0];
            GenerationError[] errors = new ProjbookEngine(new FileSystem(), "../../Projbook.Tests.csproj", configuration, ".").Generate();

            // Assert result
            Assert.IsNotNull(errors);
            Assert.AreEqual(3, errors.Length);
            Assert.AreEqual("Cannot find member: NotFound", errors[0].Message);
            Assert.AreEqual(1, errors[0].Line);
            Assert.AreEqual(52, errors[0].Column);
            Assert.AreEqual("Cannot find member: NotFound", errors[1].Message);
            Assert.AreEqual(7, errors[1].Line);
            Assert.AreEqual(52, errors[1].Column);
            Assert.IsTrue(errors[2].Message.StartsWith("Cannot find target 'Resources/FullGeneration/Source/NotFound.cs' in any referenced project"));
            Assert.AreEqual(13, errors[2].Line);
            Assert.AreEqual(12, errors[2].Column);

        }

        /// <summary>
        /// Loads a file from its path.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>The file content</returns>
        private string LoadFile(string path)
        {
            // Return null if the file doesn't exist
            if (!File.Exists(path))
            {
                return "";
            }
            
            // Otherwise return the file content
            using (StreamReader reader = new StreamReader(new FileStream(path, FileMode.Open)))
            {
                return reader.ReadToEnd().Replace("\r\n", Environment.NewLine);
            }
        }

        /// <summary>
        /// Ensures no file from tis path.
        /// </summary>
        /// <param name="path">The file path.</param>
        private void EnsureNoFile(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
