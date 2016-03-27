using System.Text;
using System.IO;
using Projbook.Extension.Spi;

namespace Projbook.Extension
{
    /// <summary>
    /// Extractor in charge of browsing source directories. load file content and extract requested member.
    /// </summary>
    public class DefaultSnippetExtractor : ISnippetExtractor
    {
        /// <summary>
        /// Extracts a snippet.
        /// </summary>
        /// <param name="streamReader">The streak reader.</param>
        /// <param name="pattern">The extraction pattern, never used for this implementation.</param>
        /// <returns>The extracted snippet.</returns>
        public virtual Model.Snippet Extract(StreamReader streamReader, string pattern)
        {
            // Extract file content
            string sourceCode = this.LoadFile(streamReader);

            // Return the entire code
            return new Model.Snippet(sourceCode);
        }

        /// <summary>
        /// Loads a file from the file name.
        /// </summary>
        /// <param name="streamReader">The streak reader.</param>
        /// <returns>The file's content.</returns>
        protected string LoadFile(StreamReader streamReader)
        {
            // Load the file content
            MemoryStream memoryStream = new MemoryStream();
            using (var fileWriter = new StreamWriter(memoryStream))
            {
                fileWriter.Write(streamReader.ReadToEnd());
            }

            // Read the code snippet from the file
            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }
    }
}