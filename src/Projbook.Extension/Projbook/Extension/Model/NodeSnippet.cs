using System;

namespace Projbook.Extension.Model
{
    /// <summary>
    /// Wraps a snippet represented as a <see cref="Node"/>.
    /// </summary>
    public class NodeSnippet : Snippet
    {
        /// <summary>
        /// The node content.
        /// </summary>
        public readonly Node Node;

        /// <summary>
        /// Initializes a new instance of <see cref="NodeSnippet"/>.
        /// </summary>
        /// <param name="node">Initializes the required <see cref="Node"/>.</param>
        public NodeSnippet(Node node)
        {
            // Data validation
            if (null == node)
                throw new ArgumentNullException("node");

            // Initialize
            this.Node = node;
        }
    }
}
