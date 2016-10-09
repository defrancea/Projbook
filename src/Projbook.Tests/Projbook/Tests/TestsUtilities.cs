using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace Projbook.Tests.Utilities
{
    /// <summary>
    /// Represents test utilities.
    /// </summary>
    public static class TestsUtilities
    {
        /// <summary>
        /// Ensures the extensions are deployed to the right directory.
        /// </summary>
        /// <returns>The extension directory.</returns>
        public static DirectoryInfoBase EnsureExtensionsDeployed()
        {
            // Create file system
            IFileSystem fileSystem = new MockFileSystem();

            // Compute directory path
            string workingDirectory = Path.GetDirectoryName(typeof(TestsUtilities).Assembly.Location);
            string directoryPath = Path.Combine(workingDirectory, "Resources", "Plugins");

            // Create extension directory if missing in both virtual and real file system
            DirectoryInfoBase extensionDirectory = fileSystem.Directory.CreateDirectory(directoryPath); ;
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            
            // Ensure extension copied
            EnsureFileCopied(workingDirectory, directoryPath, "Projbook.Extension.CSharpExtractor.dll");
            EnsureFileCopied(workingDirectory, directoryPath, "Projbook.Extension.XmlExtractor.dll");
            EnsureFileCopied(workingDirectory, directoryPath, "Projbook.Extension.FileSystemExtractor.dll");

            // Return extension directory
            return extensionDirectory;
        }
        
        /// <summary>
        /// Ensures the file is copied.
        /// </summary>
        /// <param name="sourceDirectoryPath">The source directory path.</param>
        /// <param name="targetDirectoryPath">The target directory path.</param>
        /// <param name="fileName">The file name.</param>
        private static void EnsureFileCopied(string sourceDirectoryPath, string targetDirectoryPath, string fileName)
        {
            // Compute file names
            string soureFileName = Path.Combine(sourceDirectoryPath, fileName);
            string targetFileName = Path.Combine(targetDirectoryPath, fileName);

            // Copy file is missing
            if (!File.Exists(targetFileName))
            {
                File.Copy(soureFileName, targetFileName);
            }
        }
    }
}