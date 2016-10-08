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
        /// Line pointers.
        /// </summary>
        public LinePointers LinePointers { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="PlainTextSnippet"/>.
        /// </summary>
        /// <param name="text">Initializes the required <see cref="Text"/>.</param>
        /// <param name="linePointers">Initializes the required <see cref="LinePointers"/>.</param>
        public PlainTextSnippet(string text, LinePointers linePointers = null)
        {
            // Data validation
            if (null == text)
                throw new ArgumentNullException("text");

            // Initialize
            this.Text = text;
            this.LinePointers = linePointers;
        }
    }
}