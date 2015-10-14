using EnsureThat;
using System.Text.RegularExpressions;

namespace Projbook.Core.Snippet
{
    /// <summary>
    /// Represents a snippet extraction rule.
    /// </summary>
    public class SnippetExtractionRule
    {
        /// <summary>
        /// The language.
        /// </summary>
        public string Language { get; private set; }

        /// <summary>
        /// The file name
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// The pattern.
        /// </summary>
        public string Pattern { get; private set; }

        /// <summary>
        /// Capture an expression following this format: <syntax> [<file>] <pattern>
        /// syntax:  The snippet syntax, typically a language name like xml or csharp.
        ///          This wvalue will be used for syntaax highlighting and is optional.
        ///          If no value is specified, no syntax highlighting will be applied.
        /// file:    The file where the snippe texists that have to exist either under
        ///          the current documentation project or in one of the referenced projects
        /// pattern: Syntax specific format identifying which part of the file to extract.
        ///          The value is optional an the whole file will be extracted if no patten
        ///          is definied
        /// </summary>
        private static Regex regex = new Regex(@"^([^\[\(]+)?\[([^\]]+)\](.*)$", RegexOptions.Compiled);

        /// <summary>
        /// Parses the provided expression representing snippet extraction details.
        /// </summary>
        /// <param name="snippetExtractionExpression">The snippet extraction expression.</param>
        /// <returns></returns>
        public static SnippetExtractionRule Parse(string snippetExtractionExpression)
        {
            // Data validation
            Ensure.That(() => snippetExtractionExpression).IsNotNullOrWhiteSpace();

            // Try to match the regex
            Match match = SnippetExtractionRule.regex.Match(snippetExtractionExpression);
            if (!match.Success)
            {
                return null;
            }

            // Return a new instance of snippet extraction rule
            return new SnippetExtractionRule
            {
                Language = match.Groups[1].Value.Trim(),
                FileName = match.Groups[2].Value.Trim(),
                Pattern = match.Groups[3].Value.Trim(),
            };
        }
    }
}