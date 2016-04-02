using System.Text;
using System.IO;
using Projbook.Extension.Spi;
using System;
using Projbook.Extension.Exception;

namespace Projbook.Extension
{
    /// <summary>
    /// Extractor in charge of browsing source directories. load file content and extract requested member.
    /// </summary>
    public class DefaultSnippetExtractor : ISnippetExtractor
    {
        /// <summary>
        /// File target type.
        /// </summary>
        public TargetType TargetType { get { return TargetType.File; } }

        /// <summary>
        /// Extracts a snippet.
        /// </summary>
        /// <param name="fileSystemInfo">The file system info.</param>
        /// <param name="pattern">The extraction pattern, never used for this implementation.</param>
        /// <returns>The extracted snippet.</returns>
        public virtual Model.Snippet Extract(FileSystemInfo fileSystemInfo, string pattern)
        {
            // Data validation
            if (null == fileSystemInfo)
                throw new ArgumentNullException("fileSystemInfo");
            
            // Extract file content
            string sourceCode = this.LoadFile(this.ConvertToFile(fileSystemInfo));

            // Return the entire code
            return new Model.Snippet(sourceCode);
        }

        /// <summary>
        /// Loads a file from the file name.
        /// </summary>
        /// <param name="fileInfo">The file info.</param>
        /// <returns>The file's content.</returns>
        protected string LoadFile(FileInfo fileInfo)
        {
            // Data validation
            if (null == fileInfo)
                throw new ArgumentNullException("fileInfo");

            // Load the file content
            MemoryStream memoryStream = new MemoryStream();
            using (StreamReader streamReader = new StreamReader(fileInfo.OpenRead()))
            using (var fileWriter = new StreamWriter(memoryStream))
            {
                fileWriter.Write(streamReader.ReadToEnd());
            }

            // Read the code snippet from the file
            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        /// <summary>
        /// Converts a FileSystemInfo to FileInfo.
        /// </summary>
        /// <param name="fileSystemInfo">The file system info.</param>
        /// <returns>The file info.</returns>
        protected FileInfo ConvertToFile(FileSystemInfo fileSystemInfo)
        {
            // Data validation
            if (null == fileSystemInfo)
                throw new ArgumentNullException("fileSystemInfo");

            // Cast to FileInfo
            FileInfo fileInfo = fileSystemInfo as FileInfo;
            if (null == fileInfo)
                throw new ArgumentException("fileInfo");

            // Return as file
            return fileInfo;
        }
    }
}