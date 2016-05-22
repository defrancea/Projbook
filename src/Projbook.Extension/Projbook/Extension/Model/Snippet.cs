using System;
using Projbook.Extension.Spi;

namespace Projbook.Extension.Model
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
        /// How the snippet should be rendered.
        /// </summary>
        public RenderType RenderType { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="Snippet"/>.
        /// </summary>
        /// <param name="content">Initializes the required <see cref="Content"/>.</param>
        /// <param name="renderType">Initializes the required <see cref="RenderType"/>.</param>
        public Snippet(string content, RenderType renderType = RenderType.Inject)
        {
            // Data validation
            if (null == content)
                throw new ArgumentNullException("content");

            // Initialize
            this.Content = content;
            this.RenderType = renderType;
        }
    }
}