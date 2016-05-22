using System;
using System.IO.Abstractions;
using System.Text;
using Projbook.Extension.Model;
using Projbook.Extension.Spi;

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

            // Build snippet from sub files and directories
            return this.BuildSnippet(directoryInfo);
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
        /// <param name="directoryInfo">The directory info.</param>
        /// <returns>The built snippet.</returns>
        private Snippet BuildSnippet(DirectoryInfoBase directoryInfo)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<p>" + directoryInfo.Name + ":</p>");
            stringBuilder.Append("<div class='filetree'>");
            this.BuildContent(directoryInfo, ref stringBuilder);
            stringBuilder.Append("</div>");
            return new Snippet(stringBuilder.ToString(), RenderType.Override);
        }

        /// <summary>
        /// Builds a snippet content from a directory info.
        /// </summary>
        /// <param name="directoryInfo">The directory info.</param>
        /// <param name="stringBuilder">The string builder.</param>
        private void BuildContent(DirectoryInfoBase directoryInfo, ref StringBuilder stringBuilder)
        {
            // TODO assert?

            stringBuilder.Append(directoryInfo.Name);
            stringBuilder.Append("<ul>");
            foreach (FileSystemInfoBase subFileSystemInfo in directoryInfo.GetFileSystemInfos())
            {
                stringBuilder.Append("<li>");
                FileInfoBase fileInfo = subFileSystemInfo as FileInfoBase;
                if (null != fileInfo)
                    stringBuilder.Append(fileInfo.Name);
                else
                {
                    DirectoryInfoBase subDirectoryInfo = subFileSystemInfo as DirectoryInfoBase;
                    if (null != subDirectoryInfo)
                        this.BuildContent(subDirectoryInfo, ref stringBuilder);
                    // TODO else what
                }
                stringBuilder.Append("</li>");
            }
            stringBuilder.Append("</ul>");
        }
    }
}
