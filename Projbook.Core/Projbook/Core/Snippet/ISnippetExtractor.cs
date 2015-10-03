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
        /// <returns>The extracted snippet.</returns>
        Model.Snippet Extract();
    }
}