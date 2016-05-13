using System;
using System.Collections.Generic;

namespace Projbook.Extension.Model
{
    /// <summary>
    /// A node representing a graph root.
    /// </summary>
    public class Node
    {
        /// <summary>
        /// The node name.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Whether the node is a leaf.
        /// </summary>
        public readonly bool IsLeaf;

        /// <summary>
        /// The children.
        /// </summary>
        public readonly Dictionary<string, Node> Children;

        /// <summary>
        /// Initializes a new instance of <see cref="Node"/>.
        /// </summary>
        public Node(string name, bool isLeaf)
        {
            // Data validation
            if (null == name)
                throw new ArgumentNullException("name");

            // Initialize
            this.Name = name;
            this.IsLeaf = isLeaf;
            this.Children = new Dictionary<string, Node>();
        }
    }
}