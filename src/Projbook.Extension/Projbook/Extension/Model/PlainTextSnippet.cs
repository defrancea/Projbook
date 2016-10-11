using System;

namespace Projbook.Extension.Model
{
    /// <summary>
    /// Wraps a snippet represented as a <see cref="string"/>.
    /// </summary>
    public class PlainTextSnippet : Snippet
    {
        /// <summary>
        /// The text content.
        /// </summary>
        public readonly string Text;

        /// <summary>
        /// Initializes a new instance of <see cref="PlainTextSnippet"/>.
        /// </summary>
        /// <param name="text">Initializes the required <see cref="Text"/>.</param>
        public PlainTextSnippet(string text)
        {
            // Data validation
            if (null == text)
                throw new ArgumentNullException("text");

            // Initialize
            this.Text = text;
        }
    }
}