using EnsureThat;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Projbook.Core.Snippet.CSharp
{
    /// <summary>
    /// Represents a syntax matching node.
    /// Thie node is used to build a Trie representing possible matching.
    /// Each node contians children and matching syntax nodes.
    /// </summary>
    public class CSharpSyntaxMatchingNode
    {
        /// <summary>
        /// The public Matching SyntaxNodes.
        /// </summary>
        public SyntaxNode[] MatchingSyntaxNodes
        {
            get
            {
                // Return empty array id the nodes are empty
                if (null == this.matchingSyntaxNodes)
                {
                    return new SyntaxNode[0];
                }

                // Return the matching syntax nodes
                return this.matchingSyntaxNodes.ToArray();
            }
        }

        /// <summary>
        /// The node's children.
        /// </summary>
        private Dictionary<string, CSharpSyntaxMatchingNode> children;

        /// <summary>
        /// The node's maching syntax node.
        /// </summary>
        private List<SyntaxNode> matchingSyntaxNodes;

        /// <summary>
        /// Initializes a new instance of <see cref="CSharpSyntaxMatchingNode"/>.
        /// </summary>
        public CSharpSyntaxMatchingNode()
        {
            // this.Children = new Dictionary<string, CSharpSyntaxMatchingNode>();
            // this.MatchingSyntaxNodes = new List<SyntaxNode>();
        }

        public CSharpSyntaxMatchingNode FindNode(string[] chunks)
        {
            // Data validation
            Ensure.That(() => chunks).IsNotNull();

            CSharpSyntaxMatchingNode matchingNode = this;
            foreach (string fragment in chunks)
            {
                if (!matchingNode.children.TryGetValue(fragment, out matchingNode))
                {
                    return null;
                }
            }

            return matchingNode;
        }

        public CSharpSyntaxMatchingNode AddToNode(string name)
        {
            CSharpSyntaxMatchingNode firstLevelNode;
            if (null != this.children && this.children.TryGetValue(name, out firstLevelNode))
            {
                return firstLevelNode;
            }
            else
            {
                if (null == this.children)
                {
                    this.children = new Dictionary<string, CSharpSyntaxMatchingNode>();
                }

                firstLevelNode = new CSharpSyntaxMatchingNode();
                this.children[name] = firstLevelNode;
                return firstLevelNode;
            }
        }

        public void AddSyntaxNode(SyntaxNode node)
        {
            if (null == this.matchingSyntaxNodes)
            {
                this.matchingSyntaxNodes = new List<SyntaxNode>();
            }
            this.matchingSyntaxNodes.Add(node);
        }

        public void CopyTo(CSharpSyntaxMatchingNode root, string name)
        {
            CSharpSyntaxMatchingNode n = root.AddToNode(name);
            
            // Add syntax node to the created node
            if (null != this.matchingSyntaxNodes)
            {
                if (null == n.matchingSyntaxNodes)
                {
                    n.matchingSyntaxNodes = new List<SyntaxNode>();
                }

                int[] index = n.matchingSyntaxNodes.Select(x => x.Span.Start).ToArray();
                n.matchingSyntaxNodes.AddRange(this.matchingSyntaxNodes.Where(x => !index.Contains(x.Span.Start)));
            }

            // Recurse for children
            if (null != this.children && this.children.Count > 0)
            {
                foreach (string k in this.children.Keys)
                {
                    n.CopyTo(this.children[k], k);
                }
            }
        }
    }
}