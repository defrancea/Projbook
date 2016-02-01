using System.IO;

namespace Projbook.Core.Snippet
{
    /// <summary>
    /// Defines interface for snippet extractor.
    /// </summary>
    public interface ISnippetExtractor
    {
        /// <summary>
        /// Extracts a snippet.
        /// </summary>
        /// <param name="streamReader">The streak reader.</param>
        /// <param name="pattern">The extraction pattern.</param>
        /// <returns>The extracted snippet.</returns>
        Model.Snippet Extract(StreamReader streamReader, string pattern);
    }
}