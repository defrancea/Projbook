using EnsureThat;

namespace Projbook.Core.Model
{
    /// <summary>
    /// Represents a snippet that has been extracted from source directories.
    /// </summary>
    public class Snippet
    {
        /// <summary>
        /// The snippet content.
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="Snippet"/>.
        /// </summary>
        /// <param name="content">Initializes the required <see cref="Content"/>.</param>
        public Snippet(string content)
        {
            // Data validation
            Ensure.That(() => content).IsNotNullOrWhiteSpace();

            // Initialize
            this.Content = content;
        }
    }
}