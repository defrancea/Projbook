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
        /// Extracts a snippet.
        /// </summary>
        /// <param name="streamReader">The streak reader.</param>
        /// <param name="pattern">The extraction pattern.</param>
        /// <returns>The extracted snippet.</returns>
        Snippet Extract(StreamReader streamReader, string pattern);
    }
}