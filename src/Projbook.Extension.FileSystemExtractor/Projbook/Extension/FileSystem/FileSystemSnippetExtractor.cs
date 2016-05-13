using System;
using System.IO.Abstractions;
using Projbook.Extension.Model;
using Projbook.Extension.Spi;
using System.Linq;
using System.IO;
using Projbook.Extension.Exception;

namespace Projbook.Extension.FileSystemExtractor
{
    /// <summary>
    /// Extractor in charge of browsing directories and extracts the file system tree.
    /// </summary>
    [Syntax(name: "fs")]
    public class FileSystemSnippetExtractor : ISnippetExtractor
    {
        /// <summary>
        /// Folder target type.
        /// </summary>
        public TargetType TargetType { get { return TargetType.Folder; } }

        /// <summary>
        /// Extracts a snippet.
        /// </summary>
        /// <param name="fileSystemInfo">The file system info.</param>
        /// <param name="pattern">The extraction pattern.</param>
        /// <returns>The extracted snippet.</returns>
        public Snippet Extract(FileSystemInfoBase fileSystemInfo, string pattern)
        {
            // Get the directory
            DirectoryInfoBase directoryInfo = this.ConvertToDirectory(fileSystemInfo);

            // Use * as pattern when none is set
            if (string.IsNullOrWhiteSpace(pattern))
            {
                pattern = "*";
            }

            try
            {
                // Search file system info from pattern
                FileSystemInfoBase[] fileSystemInfoBase = directoryInfo.GetFileSystemInfos(pattern, SearchOption.AllDirectories);
                
                // Build snippet
                return this.BuildSnippet(directoryInfo, fileSystemInfoBase);
            }

            // Rethrow a snippet extraction exception if the directory is not found
            catch (DirectoryNotFoundException)
            {
                throw new SnippetExtractionException("Cannot find directory", pattern);
            }
        }

        /// <summary>
        /// Converts a FileSystemInfo to DirectoryInfo.
        /// </summary>
        /// <param name="fileSystemInfo">The file system info.</param>
        /// <returns>The directory info.</returns>
        protected DirectoryInfoBase ConvertToDirectory(FileSystemInfoBase fileSystemInfo)
        {
            // Data validation
            if (null == fileSystemInfo)
                throw new ArgumentNullException("fileSystemInfo");

            // Cast to DirectoryInfo
            DirectoryInfoBase directoryInfo = fileSystemInfo as DirectoryInfoBase;
            if (null == directoryInfo)
                throw new ArgumentException("directoryInfo");

            // Return as directory
            return directoryInfo;
        }

        /// <summary>
        /// Builds a snippet from a directory info.
        /// </summary>
        /// <param name="rootDirectory">The root directory.</param>
        /// <param name="directoryInfo">The directory info.</param>
        /// <returns>The built snippet.</returns>
        private Snippet BuildSnippet(DirectoryInfoBase rootDirectory, FileSystemInfoBase[] directoryInfo)
        {
            // Compute root directory name
            string rootDirectoryName = rootDirectory.Name;
            if (string.IsNullOrWhiteSpace(rootDirectoryName))
            {
                rootDirectoryName = Path.GetFileName(rootDirectory.FullName);
            }

            // Create root node
            Node root = new Node(rootDirectoryName, false);

            // Browse all file system info matching the search pattern
            foreach (FileSystemInfoBase fileSystemInfo in directoryInfo.Where(x => rootDirectory.FullName != x.FullName))
            {
                // Compute relative path
                string relativePath = fileSystemInfo.FullName.Substring(1 + rootDirectory.FullName.Length);

                // Initialize the current node to root and process each search result
                Node currentNode = root;
                while (relativePath.Contains(Path.DirectorySeparatorChar))
                {
                    // Compute current node name from node path
                    int pos = relativePath.IndexOf(Path.DirectorySeparatorChar);
                    string nodeName = relativePath.Substring(0, pos);

                    // In the node already exists, just flip the current node
                    if (currentNode.Children.ContainsKey(nodeName))
                    {
                        currentNode = currentNode.Children[nodeName];
                    }

                    // Otherwise create a new child and flip the current node
                    else
                    {
                        Node tmpNode = new Node(nodeName, false);
                        currentNode.Children[nodeName] = tmpNode;
                        currentNode = tmpNode;
                    }

                    // Update the current relative path
                    relativePath = relativePath.Substring(1 + pos);
                }

                // Compute last filesystem info name
                string fileSystemInfoName = fileSystemInfo.Name;
                if (string.IsNullOrWhiteSpace(fileSystemInfoName))
                {
                    fileSystemInfoName = Path.GetFileName(fileSystemInfo.FullName);
                }

                // Add the remaining relative path as node
                currentNode.Children[relativePath] = new Node(fileSystemInfoName, fileSystemInfo is FileInfoBase);
            }

            // Return root as node snippet
            return new NodeSnippet(root);
        }
    }
}