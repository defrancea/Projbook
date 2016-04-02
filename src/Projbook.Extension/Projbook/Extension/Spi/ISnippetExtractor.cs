using Projbook.Extension.Model;
using System.ComponentModel.Composition;
using System.IO;

namespace Projbook.Extension.Spi
{
    /// <summary>
    /// Defines interface for snippet extractor.
    /// </summary>
    [InheritedExport]
    public interface ISnippetExtractor
    {
        /// <summary>
        /// Defines the target type.
        /// </summary>
        TargetType TargetType { get; }

        /// <summary>
        /// Extracts a snippet.
        /// </summary>
        /// <param name="fileSystemInfo">The file system info.</param>
        /// <param name="pattern">The extraction pattern.</param>
        /// <returns>The extracted snippet.</returns>
        Snippet Extract(FileSystemInfo fileSystemInfo, string pattern);
    }
}