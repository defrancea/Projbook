using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projbook.Core.Projbook.Core.Model
{
    public class Node
    {
        public Dictionary<string, Node> Nodes { get; set; }

        public List<SyntaxNode> SyntaxNodes { get; set; }

        public Node()
        {
            this.Nodes = new Dictionary<string, Node>();
            this.SyntaxNodes = new List<SyntaxNode>();
        }
    }
}
