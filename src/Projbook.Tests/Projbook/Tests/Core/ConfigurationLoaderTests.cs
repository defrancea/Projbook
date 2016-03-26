using NUnit.Framework;
using Projbook.Core;
using Projbook.Core.Exception;
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
        public override void Setup()
        {
            // Call base implementation
            base.Setup();

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
        /// <param name="readOnly">Make the configuration read only.</param>
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ValidConfiguration(bool readOnly)
        {
            // Handle file attribute
            string projectLocation = this.SourceDirectories[0].FullName;
            string configurationName = Path.Combine("Resources", "testConfig.json");
            string configurationPath = Path.Combine(projectLocation, configurationName);
            FileAttributes configurationFileAttributes = File.GetAttributes(configurationPath);

            // Make the file read only
            if (readOnly)
            {
                File.SetAttributes(configurationPath, configurationFileAttributes | FileAttributes.ReadOnly);
            }

            // Execute test
            try
            {
                Configuration[] configurations = this.ConfigurationLoader.Load(projectLocation, configurationPath);
                Assert.AreEqual("Test title", configurations[0].Title);
                Assert.AreEqual(true, configurations[0].GenerateHtml);
                Assert.AreEqual(true, configurations[0].GeneratePdf);
                Assert.IsTrue(configurations[0].TemplateHtml.EndsWith("Resources/FullGeneration/testTemplate.txt"));
                Assert.IsTrue(configurations[0].TemplatePdf.EndsWith("Resources/FullGeneration/testTemplate-pdf.txt"));
                Assert.AreEqual("testTemplate-generated.txt", configurations[0].OutputHtml);
                Assert.AreEqual("testTemplate-pdf-generated.txt", configurations[0].OutputPdf);
                Assert.AreEqual(0, configurations[0].Pages.Length);
            }
            finally
            {
                if (readOnly)
                {
                    File.SetAttributes(configurationPath, configurationFileAttributes);
                }
            }
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
            Assert.IsTrue(configurations[0].Pages[0].Path.EndsWith("Resources/FullGeneration/Page/firstPage.md"));
            Assert.AreEqual("First page title", configurations[0].Pages[0].Title);
            Assert.IsTrue(configurations[0].Pages[1].Path.EndsWith("Resources/FullGeneration/Page/secondPage.md"));
            Assert.AreEqual("Second page title", configurations[0].Pages[1].Title);
            Assert.IsTrue(configurations[0].Pages[2].Path.EndsWith("Resources/FullGeneration/Page/thirdPage.md"));
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
        /// Test configuration error reporting.
        /// </summary>
        /// <param name="configFile">The config file.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="line">The error line.</param>
        /// <param name="column">The error column.</param>
        [Test]
        [TestCase("testConfigError.json", "Unexpected end when deserializing object. Path 'title', line 2, position 24.", 2, 25)]
        [TestCase("testConfigMissingPage.json", "Could not find page 'Resources/FullGeneration/Page/missing.md'", 6, 16)]
        [TestCase("testConfigNoTemplate.json", "JSON does not match any schemas from 'anyOf'. Path '', line 3, position 1.", 3, 1)]
        [TestCase("testConfigEmptyArray.json", "Array item count 0 is less than minimum count of 1. Path '', line 2, position 1.", 2, 1)]
        public void ErrorInConfiguration(string configFile, string errorMessage, int line, int column)
        {
            // Try to generate configuration with error
            try
            {
                new ConfigurationLoader().Load(this.SourceDirectories[0].FullName, Path.Combine("Resources", configFile));
                Assert.Fail("Expected to fail");
            }
            // Assert correct error
            catch (ConfigurationException configurationException)
            {
                Assert.AreEqual("Error occured during configuration loading", configurationException.Message);
                Assert.AreEqual(1, configurationException.GenerationErrors.Length);
                Assert.AreEqual(errorMessage, configurationException.GenerationErrors[0].Message);
                Assert.AreEqual(line, configurationException.GenerationErrors[0].Line);
                Assert.AreEqual(column, configurationException.GenerationErrors[0].Column);
            }
        }

        /// <summary>
        /// Test configuration error reporting.
        /// </summary>
        [Test]
        public void NoConfiguration()
        {
            // Try to generate configuration with error
            try
            {
                new ConfigurationLoader().Load(this.SourceDirectories[0].FullName, "none.json");
                Assert.Fail("Expected to fail");
            }
            // Assert correct error
            catch (ArgumentException argumentException)
            {
                Assert.IsTrue(argumentException.Message.EndsWith(": File not found"));
            }
        }

        /// <summary>
        /// Test configuration error reporting.
        /// </summary>
        [Test]
        public void ManyErrors()
        {
            // Try to generate configuration with error
            try
            {
                new ConfigurationLoader().Load(this.SourceDirectories[0].FullName, Path.Combine("Resources", "testConfigManyErrors.json"));
                Assert.Fail("Expected to fail");
            }
            // Assert correct error
            catch (ConfigurationException argumentException)
            {
                Assert.AreEqual("Error occured during configuration loading", argumentException.Message);
                Assert.AreEqual(5, argumentException.GenerationErrors.Length);
                Assert.AreEqual("Could not find html template 'Resources/FullGeneration/notExisting.txt'", argumentException.GenerationErrors[0].Message);
                Assert.AreEqual(1, argumentException.GenerationErrors[0].Line);
                Assert.AreEqual(45, argumentException.GenerationErrors[0].Column);
                Assert.AreEqual("Could not find html template 'Resources/FullGeneration/notExisting-pdf.txt'", argumentException.GenerationErrors[1].Message);
                Assert.AreEqual(23, argumentException.GenerationErrors[1].Line);
                Assert.AreEqual(22, argumentException.GenerationErrors[1].Column);
                Assert.AreEqual("Could not find page 'Resources/FullGeneration/Page/notExisting.md'", argumentException.GenerationErrors[2].Message);
                Assert.AreEqual(7, argumentException.GenerationErrors[2].Line);
                Assert.AreEqual(18, argumentException.GenerationErrors[2].Column);
                Assert.AreEqual("Could not find page 'Resources/FullGeneration/Page/notExisting.md'", argumentException.GenerationErrors[3].Message);
                Assert.AreEqual(15, argumentException.GenerationErrors[3].Line);
                Assert.AreEqual(18, argumentException.GenerationErrors[3].Column);
                Assert.AreEqual("Could not find page 'Resources/FullGeneration/Page/anotherNotExisting.md'", argumentException.GenerationErrors[4].Message);
                Assert.AreEqual(11, argumentException.GenerationErrors[4].Line);
                Assert.AreEqual(18, argumentException.GenerationErrors[4].Column);
            }
        }
    }
}