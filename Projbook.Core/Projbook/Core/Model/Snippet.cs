using EnsureThat;

namespace Projbook.Core.Model
{
    /// <summary>
    /// Represents a snippet that has been extracted from source directories.
    /// </summary>
    public class Snippet
    {
        /// <summary>
        /// The rule used for building the snippet.
        /// </summary>
        public CSharpRule Rule { get; private set; }

        /// <summary>
        /// The snippet content.
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="Snippet"/>.
        /// </summary>
        /// <param name="rule">Initializes the required <see cref="Rule"/>.</param>
        /// <param name="content">Initializes the required <see cref="Content"/>.</param>
        public Snippet(CSharpRule rule, string content)
        {
            // Data validation
            Ensure.That(() => rule).IsNotNull();
            Ensure.That(() => content).IsNotNullOrWhiteSpace();

            // Initialize
            this.Rule = rule;
            this.Content = content;
        }
    }
}