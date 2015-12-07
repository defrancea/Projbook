using NUnit.Framework;
using System.IO;
using System.Reflection;

namespace Projbook.Tests.Core
{
    /// <summary>
    /// Abstract test.
    /// </summary>
    public abstract class AbstractTests
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
            string testAssemblyLocation = Assembly.GetAssembly(typeof(AbstractTests)).Location;
            string testAssemblyDirectory = Path.GetDirectoryName(testAssemblyLocation);
            string testSourceLocation = Path.GetFullPath(Path.Combine(testAssemblyDirectory, "..", ".."));
            this.CsprojFile = new FileInfo(Path.Combine(testSourceLocation, "Projbook.Tests.csproj"));
            this.SourceDirectories = new DirectoryInfo[] {
                new DirectoryInfo(Path.Combine(testSourceLocation)),
                new DirectoryInfo(Path.Combine(testAssemblyDirectory, "..", "..", "..", "Projbook.Core"))
            };
        }

        /// <summary>
        /// Computes the file path in the source directory.
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <returns>The file path.</returns>
        protected string ComputeFilePath(string fileName)
        {
            return Path.Combine("Resources", "SourcesA", fileName);
        }
    }
}