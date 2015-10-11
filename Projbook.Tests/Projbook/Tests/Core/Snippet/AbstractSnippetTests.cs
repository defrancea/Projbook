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
        /// The csproj file.
        /// </summary>
        protected FileInfo CsprojFile { get; private set; }

        /// <summary>
        /// The source directories.
        /// </summary>
        protected DirectoryInfo[] SourceDirectories { get; private set; }

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
            this.CsprojFile = new FileInfo(Path.Combine(testSourceLocation, "Projbook.Tests.csproj"));
            this.SourceDirectories = new DirectoryInfo[] {
                new DirectoryInfo(Path.Combine(testSourceLocation, "Resources", "SourcesA")),
                new DirectoryInfo(Path.Combine(testAssemblyDirectory, "..", "..", "..", "Projbook.Core"))
            };
        }
    }
}