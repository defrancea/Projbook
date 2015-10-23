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
        /// <param name="filePath">The file path.</param>
        /// <param name="pattern">The extraction pattern.</param>
        /// <returns>The extracted snippet.</returns>
        Model.Snippet Extract(string filePath, string pattern);
    }
}