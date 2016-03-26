using EnsureThat;
using Projbook.Core.Snippet.CSharp;
using Projbook.Core.Snippet.Xml;
using Projbook.Extension.Spi;
using System.IO;

namespace Projbook.Core.Snippet
{
    /// <summary>
    /// Creates the proper snippet exctractor depending on the pattern.
    /// </summary>
    public class SnippetExtractorFactory
    {
        /// <summary>
        /// The csproj file.
        /// </summary>
        public FileInfo CsprojFile { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="SnippetExtractorFactory"/>.
        /// </summary>
        /// <param name="csprojFile">Initializes the required <see cref="SourceDictionaries"/>.</param>
        public SnippetExtractorFactory(FileInfo csprojFile)
        {
            // Data validation
            Ensure.That(() => csprojFile).IsNotNull();
            Ensure.That(csprojFile.Exists, string.Format("Could not find '{0}' file", csprojFile)).IsTrue();

            // Initialize
            this.CsprojFile = csprojFile;
        }

        /// <summary>
        /// Creates a new instance of <see cref="ISnippetExtractor"/> according to the snippet extraction rule.
        /// </summary>
        /// <param name="snippetExtractionRule">The snippet extraction rule.</param>
        /// <returns>The matching snippet extractor.</returns>
        public ISnippetExtractor CreateExtractor(SnippetExtractionRule snippetExtractionRule)
        {
            // Return null if the extraction rule is null
            if (null == snippetExtractionRule)
            {
                return null;
            }

            // Initialize the proper extracted depending on the language
            switch (snippetExtractionRule.Language)
            {
                case "csharp":
                    return new CSharpSnippetExtractor();
                case "xml":
                    return new XmlSnippetExtractor();
                default:
                    return new DefaultSnippetExtractor();
            }
        }
    }
}