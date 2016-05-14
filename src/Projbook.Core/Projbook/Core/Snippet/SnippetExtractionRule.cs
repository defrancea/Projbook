using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EnsureThat;

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
        /// The targe path.
        /// </summary>
        public string TargetPath { get; private set; }

        /// <summary>
        /// The pattern.
        /// </summary>
        public string Pattern { get; private set; }

        /// <summary>
        /// List of data lines to highlight.
        /// </summary>
        public List<int> DataLineList { get; private set; }

        /// <summary>
        /// Capture an expression following this format: <syntax> [<file>] (<lines>) <pattern>
        /// syntax:  The snippet syntax, typically a language name like xml or csharp.
        ///          This value will be used for syntax highlighting and is optional.
        ///          If no value is specified, no syntax highlighting will be applied.
        /// file:    The file where the snippet exists that have to exist either under
        ///          the current documentation project or in one of the referenced projects
        /// lines:   The indexes of the lines to highlight
        /// pattern: Syntax specific format identifying which part of the file to extract.
        ///          The value is optional an the whole file will be extracted if no patten
        ///          is defined
        /// </summary>
        private static Regex regex = new Regex(@"^([^\[\(]+)?\[([^\]]+)\]( \(.*\))?( (.*))?$", RegexOptions.Compiled);

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

            // Remove the parentheses around the data lines
            string dataLineTxt = match.Groups[3].Value.Trim();
            if (dataLineTxt.Length >= 2)
                dataLineTxt = dataLineTxt.Substring(1, dataLineTxt.Length - 2);

            // Return a new instance of snippet extraction rule
            int i;
            return new SnippetExtractionRule
            {
                Language = match.Groups[1].Value.Trim(),
                TargetPath = match.Groups[2].Value.Trim(),
                DataLineList = dataLineTxt.Split(',').Select(s => Int32.TryParse(s, out i) ? i : 0).Where(line => line != 0).ToList(),
                Pattern = match.Groups[4].Value.Trim(),
            };
        }
    }
}