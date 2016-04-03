using NUnit.Framework;
using System.IO;
using System.IO.Abstractions;
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
        protected DirectoryInfoBase[] SourceDirectories { get; private set; }

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
            this.SourceDirectories = new DirectoryInfoBase[] {
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
            return Path.Combine("Resources", "Sources", fileName);
        }

        /// <summary>
        /// Locates a file in source directories.
        /// </summary>
        /// <param name="fileName">The file to locate.</param>
        /// <returns>The located file.</returns>
        protected FileInfoBase LocateFile(string fileName)
        {
            // Browse all directories
            foreach (DirectoryInfoBase directory in this.SourceDirectories)
            {
                // Build file name and return if existing
                FileInfoBase fileInfo = new FileInfo(Path.Combine(directory.FullName, fileName));
                if (fileInfo.Exists)
                {
                    return fileInfo;
                }
            }

            // Return false if nothing is found
            return null;
        }
    }
}