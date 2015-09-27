using NUnit.Framework;
using System.IO;
using System.Reflection;

namespace Projbook.Tests.Core.Snippet
{
    /// <summary>
    /// Abstract test for snippet extraction.
    /// </summary>
    public abstract class AbstractSnippetTests
    {
        /// <summary>
        /// The tested extractor.
        /// </summary>
        protected DirectoryInfo SourceDirectory { get; private set; }

        /// <summary>
        /// Initializes the test.
        /// </summary>
        [SetUp]
        public virtual void Setup()
        {
            // Initialize extractor
            string testAssemblyLocation = Assembly.GetAssembly(typeof(AbstractSnippetTests)).Location;
            string testAssemblyDirectory = Path.GetDirectoryName(testAssemblyLocation);
            string testSourceLocation = Path.GetFullPath(Path.Combine(testAssemblyDirectory, "..", ".."));
            this.SourceDirectory = new DirectoryInfo(Path.Combine(testSourceLocation, "Resources", "SourcesA"));
        }
    }
}