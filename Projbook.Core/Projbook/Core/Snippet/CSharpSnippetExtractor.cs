using EnsureThat;
using System.IO;
using System.Text;

namespace Projbook.Core.Snippet
{
    /// <summary>
    /// Extractor in charge of browsing source directories. load file content and extract requested member.
    /// </summary>
    public class CSharpSnippetExtractor : ISnippetExtractor
    {
        /// <summary>
        /// All source directories where snippets could possibly be.
        /// </summary>
        public DirectoryInfo[] SourceDictionaries { get; private set; }

        /// <summary>
        /// Snippet extraction pattern.
        /// </summary>
        public string Pattern { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="CSharpSnippetExtractor"/>.
        /// </summary>
        /// <param name="pattern">Initializes the required <see cref="Pattern"/>.</param>
        /// <param name="sourceDirectories">Initializes the required <see cref="SourceDictionaries"/>.</param>
        public CSharpSnippetExtractor(string pattern, params DirectoryInfo[] sourceDirectories)
        {
            // Data validation
            Ensure.That(() => pattern).IsNotNullOrWhiteSpace();
            Ensure.That(() => sourceDirectories).IsNotNull();
            Ensure.That(() => sourceDirectories).HasItems();

            // Initialize
            this.Pattern = pattern;
            this.SourceDictionaries = sourceDirectories;
        }

        /// <summary>
        /// Extracts a snippet from a given rule pattern.
        /// </summary>
        /// <param name="rule">The rule to parse and extract snippet from.</param>
        /// <returns>The extracted snippet.</returns>
        public Model.Razor.Snippet Extract()
        {
            // Load the file content
            FileInfo fileInfo = new FileInfo(Path.Combine(this.SourceDictionaries[0].FullName, this.Pattern)); // Todo: More validation and class member parsin with Roslyn
            MemoryStream memoryStream = new MemoryStream();
            using (var fileReader = new StreamReader(new FileStream(fileInfo.FullName, FileMode.Open)))
            using (var fileWriter = new StreamWriter(memoryStream))
            {
                fileWriter.Write(fileReader.ReadToEnd());
            }
            
            // Build and return the snippet
            return new Model.Razor.Snippet(Encoding.UTF8.GetString(memoryStream.ToArray()));
        }
    }
}