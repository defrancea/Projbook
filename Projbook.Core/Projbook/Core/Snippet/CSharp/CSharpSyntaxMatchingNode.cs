using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Projbook.Core.Snippet.CSharp
{
    public class CSharpSyntaxMatchingNode
    {
        public Dictionary<string, CSharpSyntaxMatchingNode> Children { get; set; }

        public List<SyntaxNode> MatchingSyntaxNodes { get; set; }

        public CSharpSyntaxMatchingNode()
        {
            this.Children = new Dictionary<string, CSharpSyntaxMatchingNode>();
            this.MatchingSyntaxNodes = new List<SyntaxNode>();
        }
    }
}