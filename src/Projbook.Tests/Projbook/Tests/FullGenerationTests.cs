using NUnit.Framework;
using Projbook.Core;
using Projbook.Core.Model;
using Projbook.Core.Model.Configuration;
using System;
using System.IO;

namespace Projbook.Tests.Core
{
    /// <summary>
    /// Perform a full generation with a testing template.
    /// </summary>
    [TestFixture]
    public class FullGenerationTests : AbstractTests
    {
        /// <summary>
        /// The WkhtmltoPdf location.
        /// </summary>
        private const string Wkhtmltopdf_Location = "../packages/wkhtmltopdf.msvc.64.exe.0.12.2.5/tools/wkhtmltopdf.exe";

        /// <summary>
        /// Run the full generation and compate the generated output with the expected output.
        /// </summary>
        /// <param name="configFileName">The config file name.</param>
        /// <param name="expectedHtmlFileName">The file name containing the expected content for HTML generation.</param>
        /// <param name="expectedPdfFileName">The file name containing the expected content for PDF generation.</param>
        /// <param name="generatedHtmlFileName">The file name containing the generated content for HTML.</param>
        /// <param name="generatedPdfFileName">The file name containing the generated content for PDF.</param>
        [Test]
        [TestCase("testConfigAllValues.json", "expected.txt", "expected-pdf.txt", "doc.html", "doc-pdf-input.html")]
        [TestCase("testConfigNoPdf.json", "expected.txt", "", "testTemplate-generated.txt", "testTemplate-pdf-generated.txt")]
        [TestCase("testConfigNoHtml.json", "", "expected-pdf.txt", "testTemplate-generated.txt", "testTemplate-pdf-generated.txt")]
        public void FullGeneration(string configFileName, string expectedHtmlFileName, string expectedPdfFileName, string generatedHtmlFileName, string generatedPdfFileName)
        {
            // Ensure nothing generated yet
            this.EnsureNoFile(generatedHtmlFileName);
            this.EnsureNoFile(generatedPdfFileName);

            // Perform generation
            Configuration configuration = new ConfigurationLoader().Load(this.SourceDirectories[0].FullName, "Resources/" + configFileName)[0];
            GenerationError[] errors = new ProjbookEngine("../../Projbook.Tests.csproj", configuration, ".", FullGenerationTests.Wkhtmltopdf_Location).Generate();

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

            // wkhtmltopdf cannot be trigerred from mono:
            // Assert.AreEqual(configuration.GeneratePdf, File.Exists(Path.ChangeExtension(configuration.OutputPdf, "pdf")));
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
            Configuration configuration = new ConfigurationLoader().Load(this.SourceDirectories[0].FullName,"Resources/" + configFileName)[0];
            GenerationError[] errors = new ProjbookEngine("../../Projbook.Tests.csproj", configuration, ".", FullGenerationTests.Wkhtmltopdf_Location).Generate();

            // Assert result
            Assert.IsNotNull(errors);
            Assert.AreEqual(2, errors.Length);
            Assert.IsTrue(errors[0].SourceFile.EndsWith(firstErrorFile));
            Assert.IsTrue(errors[1].SourceFile.EndsWith(secondErrorFile));
            Assert.AreEqual(@"Error during HTML generation: (16:1) - Encountered end tag ""Sections"" with no matching start tag.  Are your start/end tags properly balanced?", errors[0].Message);
            Assert.AreEqual(@"Error during PDF generation: (16:1) - Encountered end tag ""Sections"" with no matching start tag.  Are your start/end tags properly balanced?", errors[1].Message);
        }

        /// <summary>
        /// Run the full generation with invalid template.
        /// </summary>
        [Test]
        [TestCase]
        public void FullGenerationErrorInPdfTemplate()
        {
            // Perform generation
            Configuration configuration = new ConfigurationLoader().Load(this.SourceDirectories[0].FullName, "Resources/testConfigErrorInPdf.json")[0];
            GenerationError[] errors = new ProjbookEngine("../../Projbook.Tests.csproj", configuration, ".", FullGenerationTests.Wkhtmltopdf_Location).Generate();

            // Assert result
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Length);
            Assert.IsTrue(errors[0].SourceFile.EndsWith("testTemplateErrorInPdf-pdf.txt"));
            Assert.AreEqual(@"Error during PDF generation: (16:1) - Encountered end tag ""Sections"" with no matching start tag.  Are your start/end tags properly balanced?", errors[0].Message);

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
