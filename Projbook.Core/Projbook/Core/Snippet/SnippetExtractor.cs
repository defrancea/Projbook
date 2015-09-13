using EnsureThat;
using Projbook.Core.Model;
using System.IO;
using System.Linq;
using System.Text;

namespace Projbook.Core.Snippet
{
    /// <summary>
    /// Extractor in charge of browsing source directories. load file content and extract requested member.
    /// </summary>
    public class SnippetExtractor
    {
        /// <summary>
        /// All source directories where snippets could possibly be.
        /// </summary>
        public DirectoryInfo[] SourceDictionaries { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="SnippetExtractor"/>.
        /// </summary>
        /// <param name="sourceDirectories">Initializes the required <see cref="SourceDictionaries"/>.</param>
        public SnippetExtractor(params DirectoryInfo[] sourceDirectories)
        {
            // Data validation
            Ensure.That(() => sourceDirectories).IsNotNull();
            Ensure.That(() => sourceDirectories).HasItems();

            // Initialize
            this.SourceDictionaries = sourceDirectories;
        }

        /// <summary>
        /// Extracts a snippet from a given rule pattern.
        /// </summary>
        /// <param name="rulePattern">The rule pattern to parse and extract snippet from.</param>
        /// <returns>The extracted snippet.</returns>
        public Model.Snippet Extract(string rulePattern)
        {
            // Data validation
            Ensure.That(() => rulePattern).IsNotNullOrWhiteSpace();

            // Parses the rule pattern
            CSharpRule rule = this.ParseRule(rulePattern);
            
            // Load the file content
            MemoryStream memoryStream = new MemoryStream();
            using (var fileReader = new StreamReader(new FileStream(rule.TargetFile.FullName, FileMode.Open)))
            using (var fileWriter = new StreamWriter(memoryStream))
            {
                fileWriter.Write(fileReader.ReadToEnd());
            }
            
            // Build and return the snippet
            return new Model.Snippet(
                rule: rule,
                content: Encoding.UTF8.GetString(memoryStream.ToArray()));
        }

        /// <summary>
        /// Parses the rule pattern into a rule.
        /// </summary>
        /// <param name="rulePattern">The rule pattern as string to parse.</param>
        /// <returns>The parsed rule.</returns>
        private CSharpRule ParseRule(string rulePattern)
        {
            // Data validation
            Ensure.That(() => rulePattern).IsNotNullOrWhiteSpace();

            // Todo: Implemented property but simple consider the pattern as the file path for now
            return new CSharpRule(Path.Combine(this.SourceDictionaries.First().FullName, rulePattern));
        }
    }
}