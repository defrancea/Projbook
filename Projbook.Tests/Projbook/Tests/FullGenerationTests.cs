using NUnit.Framework;
using Projbook.Core;
using Projbook.Core.Model;
using System;
using System.IO;

namespace Projbook.Tests.Core
{
    /// <summary>
    /// Perform a full generation with a testing template.
    /// </summary>
    [TestFixture]
    public class FullGenerationTests
    {
        /// <summary>
        /// Run the full generation and compate the generated output with the expected output.
        /// </summary>
        /// <param name="templateFileName">The source template file name.</param>
        /// <param name="expectedHtmlFileName">The file name containing the expected content for HTML generation.</param>
        /// <param name="expectedPdfFileName">The file name containing the expected content for PDF generation.</param>
        /// <param name="generatedHtmlFileName">The file name containing the generated content for HTML.</param>
        /// <param name="generatedPdfFileName">The file name containing the generated content for PDF.</param>
        [Test]
        [TestCase("testTemplate.txt", "expected.txt", "expected-pdf.txt", "testTemplate-generated.txt", "testTemplate-pdf-generated.txt")]
        [TestCase("testTemplateNoPdf.txt", "expected.txt", "expected.txt", "testTemplateNoPdf-generated.txt", "testTemplateNoPdf-pdf-generated.txt")]
        public void FullGeneration(string templateFileName, string expectedHtmlFileName, string expectedPdfFileName, string generatedHtmlFileName, string generatedPdfFileName)
        {
            // Perform generation
            GenerationError[] errors = new ProjbookEngine("../../Projbook.Tests.csproj", "Resources/FullGeneration/" + templateFileName, "Resources/testConfig.json", ".").Generate();

            // Read expected ouput
            string expectedContent = this.LoadFile("Resources/FullGeneration/" + expectedHtmlFileName);

            // Read expected ouput
            string expectedPdfContent = this.LoadFile("Resources/FullGeneration/" + expectedPdfFileName);

            // Read generated ouput
            string generatedContent = this.LoadFile(generatedHtmlFileName);

            // Read generated pdf ouput
            string generatedPdfContent = this.LoadFile(generatedPdfFileName);

            // Assert result
            Assert.IsNotNull(errors);
            Assert.AreEqual(0, errors.Length);
            Assert.AreEqual(expectedContent, generatedContent);
            Assert.AreEqual(expectedPdfContent, generatedPdfContent);
        }

        /// <summary>
        /// Run the full generation with invalid template.
        /// </summary>
        /// <param name="templateFileName">The source template file name.</param>
        /// <param name="firstErrorFile">The file where the first error occured.</param>
        /// <param name="secondErrorFile">The file where the second error occured.</param>
        [Test]
        [TestCase("testTemplateError.txt", "testTemplateError.txt", "testTemplateError-pdf.txt")]
        [TestCase("testTemplateErrorNoPdf.txt", "testTemplateErrorNoPdf.txt", "testTemplateErrorNoPdf.txt")]
        public void FullGenerationErrorTemplate(string templateFileName, string firstErrorFile, string secondErrorFile)
        {
            // Perform generation
            GenerationError[] errors = new ProjbookEngine("../../Projbook.Tests.csproj", "Resources/FullGeneration/" + templateFileName, "Resources/testConfig.json", ".").Generate();

            // Assert result
            Assert.IsNotNull(errors);
            Assert.AreEqual(2, errors.Length);
            Assert.IsTrue(errors[0].SourceFile.EndsWith(firstErrorFile));
            Assert.IsTrue(errors[1].SourceFile.EndsWith(secondErrorFile));
            Assert.AreEqual(@"Error during HTML generation: (17:4) - Encountered end tag ""Anchors"" with no matching start tag.  Are your start/end tags properly balanced?", errors[0].Message);
            Assert.AreEqual(@"Error during PDF generation: (17:4) - Encountered end tag ""Anchors"" with no matching start tag.  Are your start/end tags properly balanced?", errors[1].Message);
        }

        /// <summary>
        /// Run the full generation with invalid template.
        /// </summary>
        /// <param name="templateFileName">The source template file name.</param>
        /// <param name="errorFile">The file where the first error occured.</param>
        [Test]
        [TestCase("testTemplateErrorInPdf.txt", "testTemplateErrorInPdf-pdf.txt")]
        public void FullGenerationErrorInPdfTemplate(string templateFileName, string errorFile)
        {
            // Perform generation
            GenerationError[] errors = new ProjbookEngine("../../Projbook.Tests.csproj", "Resources/FullGeneration/" + templateFileName, "Resources/testConfig.json", ".").Generate();

            // Assert result
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Length);
            Assert.IsTrue(errors[0].SourceFile.EndsWith(errorFile));
            Assert.AreEqual(@"Error during PDF generation: (17:4) - Encountered end tag ""Anchors"" with no matching start tag.  Are your start/end tags properly balanced?", errors[0].Message);
        }

        /// <summary>
        /// Run the full generation with invalid template.
        /// </summary>
        /// <param name="configFile">The config file.</param>
        /// <param name="errorFile">The file where the first error occured.</param>
        /// <param name="errorMessage">The error message.</param>
        [Test]
        [TestCase("testConfigError.json", "Error during loading configuration: Unexpected end when deserializing object. Path 'title', line 2, position 25.")]
        [TestCase("testConfigMissingPage.json", "Error during loading configuration: Could not find file ")]
        public void FullGenerationErrorInConfiguration(string configFile, string errorMessage)
        {
            // Perform generation
            GenerationError[] errors = new ProjbookEngine("../../Projbook.Tests.csproj", "Resources/FullGeneration/testTemplate.txt", "Resources/" + configFile, ".").Generate();

            // Assert result
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Length);
            Assert.IsTrue(errors[0].SourceFile.EndsWith(configFile));
            Console.WriteLine(errors[0].Message);
            Assert.IsTrue(errors[0].Message.StartsWith(errorMessage));
        }

        /// <summary>
        /// Loads a file from its path.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>The file content</returns>
        private string LoadFile(string path)
        {
            using (StreamReader reader = new StreamReader(new FileStream(path, FileMode.Open)))
            {
                return reader.ReadToEnd().Replace("\r\n", Environment.NewLine);
            }
        }
    }
}
