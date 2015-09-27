using NUnit.Framework;
using Projbook.Core.Model.Configuration;
using Projbook.Core.Projbook.Core;
using System;
using System.IO;

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
            this.ConfigurationLoader.Load(null);
        }

        /// <summary>
        /// Tests with invalid input.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void WrongInitNotFound()
        {
            this.ConfigurationLoader.Load(new FileInfo(""));
            this.ConfigurationLoader.Load(new FileInfo("does not exist"));
        }

        /// <summary>
        /// Tests with valid configuration.
        /// </summary>
        [Test]
        public void ValidConfiguration()
        {
            Configuration configuration = this.ConfigurationLoader.Load(new FileInfo(Path.Combine("Resources", "testConfig.json")));
            Assert.AreEqual("Simple title", configuration.Title);
            Assert.AreEqual(2, configuration.Pages.Length);
            Assert.AreEqual("firstPage.md", configuration.Pages[0].Path);
            Assert.AreEqual("First page title", configuration.Pages[0].Title);
            Assert.AreEqual("secondPage.md", configuration.Pages[1].Path);
            Assert.AreEqual("Second page title", configuration.Pages[1].Title);
        }
    }
}