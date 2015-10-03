using EnsureThat;
using Projbook.Core.Snippet.CSharp;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Projbook.Core.Snippet
{
    /// <summary>
    /// Creates the proper snippet exctractor depending on the pattern.
    /// </summary>
    public class SnippetExtractorFactory
    {
        /// <summary>
        /// All source directories where snippets could possibly be.
        /// </summary>
        public DirectoryInfo[] SourceDictionaries { get; private set; }

        /// <summary>
        /// Regex extracting the language and snippet extraction pattern.
        /// </summary>
        private static Regex regex = new Regex(@"^(.+)\[(.+)\]$", RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of <see cref="SnippetExtractorFactory"/>.
        /// </summary>
        /// <param name="sourceDirectories">Initializes the required <see cref="SourceDictionaries"/>.</param>
        public SnippetExtractorFactory(params DirectoryInfo[] sourceDirectories)
        {
            // Data validation
            Ensure.That(() => sourceDirectories).IsNotNull();
            Ensure.That(() => sourceDirectories).HasItems();

            // Initialize
            this.SourceDictionaries = sourceDirectories;
        }

        /// <summary>
        /// Creates a new instance of <see cref="ISnippetExtractor"/> according to the snippet extraction rule.
        /// </summary>
        /// <param name="snippetReference"></param>
        /// <returns>The matching snippet extractor. If </returns>
        public ISnippetExtractor CreateExtractor(string snippetReference)
        {
            // Data validation
            Ensure.That(() => snippetReference).IsNotNullOrWhiteSpace();

            // Match pattern
            Match match = regex.Match(snippetReference);
            if (match.Success)
            {
                // Retrieve code language and rule
                string language = match.Groups[1].Value;
                string pattern = match.Groups[2].Value;

                // Initialize the proper extracted depending on the language
                switch (language)
                {
                    case "csharp":
                        return new CSharpSnippetExtractor(pattern, this.SourceDictionaries);
                    default:
                        throw new NotSupportedException(string.Format("Could not create extractor for '{0}': language unknown", language));
                }
            }

            // No match found
            else
            {
                return null;
            }
        }
    }
}