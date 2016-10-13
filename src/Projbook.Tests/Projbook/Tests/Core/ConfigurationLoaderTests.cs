﻿using NUnit.Framework;
using Projbook.Core;
using Projbook.Core.Exception;
using Projbook.Core.Model.Configuration;
using Projbook.Tests.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace Projbook.Tests.Core
{
    /// <summary>
    /// Tests <see cref="ConfigurationLoader"/>.
    /// </summary>
    [TestFixture]
    public class ConfigurationLoaderTests
    {
        /// <summary>
        /// Configuration loader.
        /// </summary>
        public ConfigurationLoader ConfigurationLoader { get; private set; }

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
                { "Config/Simple.json", new MockFileData(ConfigFiles.Simple) },
                { "Config/AllValues.json", new MockFileData(ConfigFiles.AllValues) },
                { "Config/TwoGenerations.json", new MockFileData(ConfigFiles.TwoGenerations) },
                { "Config/WithIndex.json", new MockFileData(ConfigFiles.WithIndex) },
                { "Config/ErrorInIndexHtml.json", new MockFileData(ConfigFiles.ErrorInIndexHtml) },
                { "Config/Error.json", new MockFileData(ConfigFiles.Error) },
                { "Config/MissingPage.json", new MockFileData(ConfigFiles.MissingPage) },
                { "Config/NoTemplate.json", new MockFileData(ConfigFiles.NoTemplate) },
                { "Config/EmptyArray.json", new MockFileData(ConfigFiles.EmptyArray) },
                { "Config/ManyErrors.json", new MockFileData(ConfigFiles.ManyErrors) },
                { "Template/Template.txt", new MockFileData("Template") },
                { "Template/IndexTemplate.txt", new MockFileData("Template") },
                { "Template/Malformated.txt", new MockFileData("Template") },
                { "Template/Template-pdf.txt", new MockFileData("Template Pdf") },
                { "Page/Content.md", new MockFileData(PageFiles.Content) },
                { "Page/Snippet.md", new MockFileData(PageFiles.Snippet) },
                { "Page/Table.md", new MockFileData(PageFiles.Table) }
            });

            // Initialize configuration loader
            this.ConfigurationLoader = new ConfigurationLoader(this.FileSystem);
        }

        /// <summary>
        /// Tests with invalid input.
        /// </summary>
        [Test]
        public void WrongInitNull()
        {
            Assert.Throws(
                Is.TypeOf<ArgumentNullException>(), 
                () => this.ConfigurationLoader.Load(".", null));
        }

        /// <summary>
        /// Tests with invalid input.
        /// </summary>
        [Test]
        public void WrongInitEmpty()
        {
            Assert.Throws(
                Is.TypeOf<ArgumentException>(),
                () => this.ConfigurationLoader.Load(".", ""));
        }

        /// <summary>
        /// Tests with invalid input.
        /// </summary>
        [Test]
        public void WrongInitNotFound()
        {
            Assert.Throws(
                Is.TypeOf<ArgumentException>(),
                () => this.ConfigurationLoader.Load(".", "does not exist"));
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
            // Declare configuration name
            string fileName = this.FileSystem.Path.Combine("Config", "Simple.json");

            // Handle file attribute
            FileAttributes configurationFileAttributes = this.FileSystem.File.GetAttributes(fileName);

            // Make the file read only
            if (readOnly)
            {
                this.FileSystem.File.SetAttributes(fileName, configurationFileAttributes | FileAttributes.ReadOnly);
            }

            // Load configuration
            IndexConfiguration indexConfiguration = this.ConfigurationLoader.Load(".", fileName);
            Configuration[] configurations = indexConfiguration.Configurations;

            // Assert
            Assert.AreEqual("Test title", configurations[0].Title);
            Assert.AreEqual(true, configurations[0].GenerateHtml);
            Assert.AreEqual(true, configurations[0].GeneratePdf);
            Assert.IsTrue(configurations[0].TemplateHtml.EndsWith("Template/Template.txt".Replace('/', this.FileSystem.Path.DirectorySeparatorChar)));
            Assert.IsTrue(configurations[0].TemplatePdf.EndsWith("Template/Template-pdf.txt".Replace('/', this.FileSystem.Path.DirectorySeparatorChar)));
            Assert.AreEqual("Template-generated.txt", configurations[0].OutputHtml);
            Assert.AreEqual("Template-pdf-generated.txt", configurations[0].OutputPdf);
            Assert.AreEqual(0, configurations[0].Pages.Length);
        }

        /// <summary>
        /// Tests with valid configuration with all values.
        /// </summary>
        [Test]
        public void ValidConfigurationAllValues()
        {
            // Declare configuration name
            string fileName = this.FileSystem.Path.Combine("Config", "AllValues.json");

            // Load configuration
            IndexConfiguration indexConfiguration = this.ConfigurationLoader.Load(".", fileName);
            Configuration[] configurations = indexConfiguration.Configurations;

            // Assert
            Assert.AreEqual("Test title", configurations[0].Title);
            Assert.AreEqual("Test description", configurations[0].Description);
            Assert.AreEqual("icon.png", configurations[0].Icon);
            Assert.AreEqual(true, configurations[0].GenerateHtml);
            Assert.AreEqual(true, configurations[0].GeneratePdf);
            Assert.IsTrue(configurations[0].TemplateHtml.EndsWith("Template.txt"));
            Assert.IsTrue(configurations[0].TemplatePdf.EndsWith("Template-pdf.txt"));
            Assert.AreEqual("doc.html", configurations[0].OutputHtml);
            Assert.AreEqual("doc-pdf-input.html", configurations[0].OutputPdf);
            Assert.AreEqual(3, configurations[0].Pages.Length);
            Assert.IsTrue(configurations[0].Pages[0].Path.EndsWith("Page/Content.md"));
            Assert.AreEqual("First page title", configurations[0].Pages[0].Title);
            Assert.IsTrue(configurations[0].Pages[1].Path.EndsWith("Page/Snippet.md"));
            Assert.AreEqual("Second page title", configurations[0].Pages[1].Title);
            Assert.IsTrue(configurations[0].Pages[2].Path.EndsWith("Page/Table.md"));
            Assert.AreEqual("Third page title", configurations[0].Pages[2].Title);
        }

        /// <summary>
        /// Tests with valid configuration with all values and two generations.
        /// </summary>
        [Test]
        public void ValidConfigurationTwoGenerationsWithHtmlOnlyAndPdfOnly()
        {
            // Declare configuration name
            string fileName = this.FileSystem.Path.Combine("Config", "TwoGenerations.json");

            // Load configuration
            IndexConfiguration indexConfiguration = this.ConfigurationLoader.Load(".", fileName);
            Configuration[] configurations = indexConfiguration.Configurations;

            // Assert
            Assert.AreEqual("Test title 1", configurations[0].Title);
            Assert.AreEqual(true, configurations[0].GenerateHtml);
            Assert.AreEqual(false, configurations[0].GeneratePdf);
            Assert.IsTrue(configurations[0].TemplateHtml.EndsWith("Template.txt"));
            Assert.AreEqual("Template-generated.txt", configurations[0].OutputHtml);
            Assert.AreEqual(null, configurations[0].TemplatePdf);
            Assert.AreEqual(null, configurations[0].OutputPdf);
            Assert.AreEqual(0, configurations[0].Pages.Length);
            Assert.AreEqual("Test title 2", configurations[1].Title);
            Assert.AreEqual(false, configurations[1].GenerateHtml);
            Assert.AreEqual(true, configurations[1].GeneratePdf);
            Assert.AreEqual(null, configurations[1].TemplateHtml);
            Assert.AreEqual(null, configurations[1].OutputHtml);
            Assert.IsTrue(configurations[1].TemplatePdf.EndsWith("Template-pdf.txt"));
            Assert.AreEqual("Template-pdf-generated.txt", configurations[1].OutputPdf);
            Assert.AreEqual(0, configurations[1].Pages.Length);
        }

        /// <summary>
        /// Tests with valid configuration with index.
        /// </summary>
        [Test]
        public void ValidConfigurationWithIndex()
        {
            // Declare configuration name
            string fileName = this.FileSystem.Path.Combine("Config", "WithIndex.json");

            // Load configuration
            IndexConfiguration indexConfiguration = this.ConfigurationLoader.Load(".", fileName);
            Configuration[] configurations = indexConfiguration.Configurations;

            // Assert
            Assert.AreEqual("Index title", indexConfiguration.Title);
            Assert.AreEqual("Index description", indexConfiguration.Description);
            Assert.IsNotNull(indexConfiguration.Template);
            Assert.AreEqual("index.html", indexConfiguration.Output);
            Assert.AreEqual("index-icon.png", indexConfiguration.Icon);
            Assert.AreEqual(1, configurations.Length);
        }

        /// <summary>
        /// Tests with valid configuration with index.
        /// </summary>
        [Test]
        public void ValidDefaultIndexOutputName()
        {
            // Declare configuration name
            string fileName = this.FileSystem.Path.Combine("Config", "ErrorInIndexHtml.json");

            // Load configuration
            IndexConfiguration indexConfiguration = this.ConfigurationLoader.Load(".", fileName);
            Configuration[] configurations = indexConfiguration.Configurations;

            // Assert
            Assert.AreEqual("Malformated-generated.txt", indexConfiguration.Output);
        }

        /// <summary>
        /// Test configuration error reporting.
        /// </summary>
        /// <param name="configFile">The config file.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="line">The error line.</param>
        /// <param name="column">The error column.</param>
        [Test]
        [TestCase("Config/Error.json", "Unexpected end when deserializing object. Path 'title', line 2, position 24.", 2, 25)]
        [TestCase("Config/MissingPage.json", "Could not find page 'Page/Missing.md'", 6, 16)]
        [TestCase("Config/NoTemplate.json", "Required properties are missing from object: template. Path '', line 3, position 1.", 3, 1)]
        [TestCase("Config/EmptyArray.json", "Array item count 0 is less than minimum count of 1. Path '', line 2, position 1.", 2, 1)]
        public void ErrorInConfiguration(string configFile, string errorMessage, int line, int column)
        {
            // Try to generate configuration with error
            try
            {
                configFile = configFile.Replace('/', this.FileSystem.Path.DirectorySeparatorChar);
                this.ConfigurationLoader.Load(".", configFile);
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
                new ConfigurationLoader(new FileSystem()).Load(".", "None.json");
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
                // Declare configuration name
                string fileName = this.FileSystem.Path.Combine("Config", "ManyErrors.json");

                // Load configuration
                IndexConfiguration indexConfiguration = this.ConfigurationLoader.Load(".", fileName);
                Configuration[] configurations = indexConfiguration.Configurations;

                // Assert
                Assert.Fail("Expected to fail");
            }
            // Assert correct error
            catch (ConfigurationException argumentException)
            {
                Assert.AreEqual("Error occured during configuration loading", argumentException.Message);
                Assert.AreEqual(6, argumentException.GenerationErrors.Length);
                Assert.AreEqual("Could not find index template 'Template/NotExisting.txt'", argumentException.GenerationErrors[0].Message);
                Assert.AreEqual(3, argumentException.GenerationErrors[0].Line);
                Assert.AreEqual(16, argumentException.GenerationErrors[0].Column);
                Assert.AreEqual("Could not find html template 'Template/NotExisting.txt'", argumentException.GenerationErrors[1].Message);
                Assert.AreEqual(7, argumentException.GenerationErrors[1].Line);
                Assert.AreEqual(25, argumentException.GenerationErrors[1].Column);
                Assert.AreEqual("Could not find html template 'Template/NotExisting-pdf.txt'", argumentException.GenerationErrors[2].Message);
                Assert.AreEqual(29, argumentException.GenerationErrors[2].Line);
                Assert.AreEqual(24, argumentException.GenerationErrors[2].Column);
                Assert.AreEqual("Could not find page 'Page/NotExisting.md'", argumentException.GenerationErrors[3].Message);
                Assert.AreEqual(13, argumentException.GenerationErrors[3].Line);
                Assert.AreEqual(20, argumentException.GenerationErrors[3].Column);
                Assert.AreEqual("Could not find page 'Page/NotExisting.md'", argumentException.GenerationErrors[4].Message);
                Assert.AreEqual(21, argumentException.GenerationErrors[4].Line);
                Assert.AreEqual(20, argumentException.GenerationErrors[4].Column);
                Assert.AreEqual("Could not find page 'Page\\AnotherNotExisting.md'", argumentException.GenerationErrors[5].Message);
                Assert.AreEqual(17, argumentException.GenerationErrors[5].Line);
                Assert.AreEqual(20, argumentException.GenerationErrors[5].Column);
            }
        }
    }
}