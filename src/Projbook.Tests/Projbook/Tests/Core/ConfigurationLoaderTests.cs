using NUnit.Framework;
using Projbook.Core;
using Projbook.Core.Model.Configuration;
using System;
using System.IO;

namespace Projbook.Tests.Core
{
    /// <summary>
    /// Tests <see cref="ConfigurationLoader"/>.
    /// </summary>
    [TestFixture]
    public class ConfigurationLoaderTests : AbstractTests
    {
        /// <summary>
        /// Configuration loader.
        /// </summary>
        public ConfigurationLoader ConfigurationLoader { get; private set; }

        /// <summary>
        /// Initializes the test.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Initialize configuration loader
            this.ConfigurationLoader = new ConfigurationLoader();
        }

        /// <summary>
        /// Tests with invalid input.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WrongInitNull()
        {
            this.ConfigurationLoader.Load(this.SourceDirectories[0].FullName, null);
        }

        /// <summary>
        /// Tests with invalid input.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void WrongInitEmpty()
        {
            this.ConfigurationLoader.Load(this.SourceDirectories[0].FullName, "");
        }

        /// <summary>
        /// Tests with invalid input.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void WrongInitNotFound()
        {
            this.ConfigurationLoader.Load(this.SourceDirectories[0].FullName, "does not exist");
        }

        /// <summary>
        /// Tests with valid configuration.
        /// </summary>
        [Test]
        public void ValidConfiguration()
        {
            Configuration[] configurations = this.ConfigurationLoader.Load(this.SourceDirectories[0].FullName, Path.Combine("Resources", "testConfig.json"));
            Assert.AreEqual("Test title", configurations[0].Title);
            Assert.AreEqual(true, configurations[0].GenerateHtml);
            Assert.AreEqual(true, configurations[0].GeneratePdf);
            Assert.IsTrue(configurations[0].TemplateHtml.EndsWith("Resources/FullGeneration/testTemplate.txt"));
            Assert.IsTrue(configurations[0].TemplatePdf.EndsWith("Resources/FullGeneration/testTemplate-pdf.txt"));
            Assert.AreEqual("testTemplate-generated.txt", configurations[0].OutputHtml);
            Assert.AreEqual("testTemplate-pdf-generated.txt", configurations[0].OutputPdf);
            Assert.AreEqual(0, configurations[0].Pages.Length);
        }

        /// <summary>
        /// Tests with valid configuration with all values.
        /// </summary>
        [Test]
        public void ValidConfigurationAllValues()
        {
            Configuration[] configurations = this.ConfigurationLoader.Load(this.SourceDirectories[0].FullName, Path.Combine("Resources", "testConfigAllValues.json"));
            Assert.AreEqual("Test title", configurations[0].Title);
            Assert.AreEqual(true, configurations[0].GenerateHtml);
            Assert.AreEqual(true, configurations[0].GeneratePdf);
            Assert.IsTrue(configurations[0].TemplateHtml.EndsWith("Resources/FullGeneration/testTemplate.txt"));
            Assert.IsTrue(configurations[0].TemplatePdf.EndsWith("Resources/FullGeneration/testTemplate-pdf.txt"));
            Assert.AreEqual("doc.html", configurations[0].OutputHtml);
            Assert.AreEqual("doc-pdf-input.html", configurations[0].OutputPdf);
            Assert.AreEqual(3, configurations[0].Pages.Length);
            Assert.AreEqual("Resources/FullGeneration/Page/firstPage.md", configurations[0].Pages[0].Path);
            Assert.AreEqual("First page title", configurations[0].Pages[0].Title);
            Assert.AreEqual("Resources/FullGeneration/Page/secondPage.md", configurations[0].Pages[1].Path);
            Assert.AreEqual("Second page title", configurations[0].Pages[1].Title);
            Assert.AreEqual("Resources/FullGeneration/Page/thirdPage.md", configurations[0].Pages[2].Path);
            Assert.AreEqual("Third page title", configurations[0].Pages[2].Title);
        }

        /// <summary>
        /// Tests with valid configuration with all values and two generations.
        /// </summary>
        [Test]
        public void ValidConfigurationTwoGenerationsWithHtmlOnlyAndPdfOnly()
        {
            Configuration[] configurations = this.ConfigurationLoader.Load(this.SourceDirectories[0].FullName, Path.Combine("Resources", "testConfigTwoGenerations.json"));
            Assert.AreEqual("Test title 1", configurations[0].Title);
            Assert.AreEqual(true, configurations[0].GenerateHtml);
            Assert.AreEqual(false, configurations[0].GeneratePdf);
            Assert.IsTrue(configurations[0].TemplateHtml.EndsWith("Resources/FullGeneration/testTemplate.txt"));
            Assert.AreEqual("testTemplate-generated.txt", configurations[0].OutputHtml);
            Assert.AreEqual(null, configurations[0].TemplatePdf);
            Assert.AreEqual(null, configurations[0].OutputPdf);
            Assert.AreEqual(0, configurations[0].Pages.Length);
            Assert.AreEqual("Test title 2", configurations[1].Title);
            Assert.AreEqual(false, configurations[1].GenerateHtml);
            Assert.AreEqual(true, configurations[1].GeneratePdf);
            Assert.AreEqual(null, configurations[1].TemplateHtml);
            Assert.AreEqual(null, configurations[1].OutputHtml);
            Assert.IsTrue(configurations[1].TemplatePdf.EndsWith("Resources/FullGeneration/testTemplate-pdf.txt"));
            Assert.AreEqual("testTemplate-pdf-generated.txt", configurations[1].OutputPdf);
            Assert.AreEqual(0, configurations[1].Pages.Length);
        }
        
        /// <summary>
        /// Run the full generation with invalid template.
        /// </summary>
        /// <param name="configFile">The config file.</param>
        /// <param name="errorMessage">The error message.</param>
        [Test]
        [TestCase("none.json", "none.json': File not found")]
        [TestCase("testConfigError.json", "Unexpected end when deserializing object. Path 'title', line 2, position 25.")]
        [TestCase("testConfigMissingPage.json", "Could not find page 'Resources/FullGeneration/Page/missing.md'")]
        [TestCase("testConfigNoTemplate.json", "At least one template must to be definied, possible template configuration: template-html, template-pdf")]
        [TestCase("testConfigEmptyArray.json", "Could not find configuration definition")]
        public void ErrorInConfiguration(string configFile, string errorMessage)
        {
            // Try to generate configuration with error
            try
            {
                new ConfigurationLoader().Load(this.SourceDirectories[0].FullName, Path.Combine("Resources", configFile));
                Assert.Fail("Expected to fail");
            }

            // Assert correct error
            catch (Exception exception)
            {
                Assert.IsTrue(exception.Message.EndsWith(errorMessage));
            }
        }
    }
}