using NUnit.Framework;
using Projbook.Core;
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
        [Test]
        [TestCase]
        public void FullGeneration()
        {
            // Perform generation
            new ProjbookEngine("../..", "Resources/FullGeneration/testTemplate.txt", "Resources/testConfig.json", ".").Generate();

            // Read expected ouput
            string expectedContent = this.LoadFile("Resources/FullGeneration/expected.txt");

            // Read expected ouput
            string expectedPdfContent = this.LoadFile("Resources/FullGeneration/expected-pdf.txt");

            // Read generated ouput
            string generatedContent = this.LoadFile("testTemplate-generated.txt");

            // Read generated pdf ouput
            string generatedPdfContent = this.LoadFile("testTemplate-pdf-generated.txt");

            // Assert contents
            Assert.AreEqual(expectedContent, generatedContent);
            Assert.AreEqual(expectedPdfContent, generatedPdfContent);
        }

        private string LoadFile(string path)
        {
            using (StreamReader reader = new StreamReader(new FileStream(path, FileMode.Open)))
            {
                return reader.ReadToEnd().Replace("\r\n", Environment.NewLine);
            }
        }
    }
}
