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
            string expectedContent;
            using (StreamReader reader = new StreamReader(new FileStream("Resources/FullGeneration/expected.txt", FileMode.Open)))
            {
                expectedContent = reader.ReadToEnd().Replace("\r\n", Environment.NewLine);
            }

            // Read generated ouput
            string generatedContent;
            using (StreamReader reader = new StreamReader(new FileStream("generated.html", FileMode.Open)))
            {
                generatedContent = reader.ReadToEnd().Replace("\r\n", Environment.NewLine);
            }

            // Assert contents
            Assert.AreEqual(expectedContent, generatedContent);
        }
    }
}
