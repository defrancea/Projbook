using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projbook.Core.Snippet.CSharp
{
    class CSharpSyntaxWalkerMatchingBuilder : CSharpSyntaxWalker
    {
        public CSharpSyntaxMatchingNode Root { get; set; }

        public CSharpSyntaxMatchingNode InvariantRoot { get; set; }

        public CSharpSyntaxWalkerMatchingBuilder()
        {
            this.Root = new CSharpSyntaxMatchingNode();
            this.InvariantRoot = this.Root;

        }

        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            string name = node.Name.ToString();
            string[] ns = name.Split('.');
            CSharpSyntaxMatchingNode initialNode = this.Root;

            CSharpSyntaxMatchingNode startNsNode = null;

            foreach (string n in ns)
            {
                this.Root = this.AddToNode(this.Root, n);
                if (null == startNsNode)
                {
                    startNsNode = this.Root;
                }
            }
            this.Root.MatchingSyntaxNodes.Add(node);

            base.VisitNamespaceDeclaration(node);
            this.CopyTo(this.InvariantRoot, startNsNode, ns[0]);
            this.Root = initialNode;
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            string name = node.Identifier.ValueText;
            CSharpSyntaxMatchingNode initialNode = this.Root;

            this.Root = this.AddToNode(this.Root, name);
            this.Root.MatchingSyntaxNodes.Add(node);

            base.VisitClassDeclaration(node);
            this.CopyTo(this.InvariantRoot, this.Root, name);
            this.Root = initialNode;
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            string name = node.Identifier.ValueText;
            CSharpSyntaxMatchingNode initialNode = this.Root;

            this.Root = this.AddToNode(this.Root, name);
            this.Root.MatchingSyntaxNodes.Add(node);

            base.VisitPropertyDeclaration(node);
            this.CopyTo(this.InvariantRoot, this.Root, name);
            this.Root = initialNode;
        }

        public override void VisitAccessorDeclaration(AccessorDeclarationSyntax node)
        {
            string name = node.Keyword.ValueText;
            CSharpSyntaxMatchingNode initialNode = this.Root;

            this.Root = this.AddToNode(this.Root, name);
            this.Root.MatchingSyntaxNodes.Add(node);

            base.VisitAccessorDeclaration(node);
            this.CopyTo(this.InvariantRoot, this.Root, name);
            this.Root = initialNode;
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            string name = node.Identifier.ValueText;
            CSharpSyntaxMatchingNode initialNode = this.Root;

            this.Root = this.AddToNode(this.Root, name);
            this.Root.MatchingSyntaxNodes.Add(node);

            base.VisitMethodDeclaration(node);
            this.CopyTo(this.InvariantRoot, this.Root, name);
            this.Root = initialNode;
        }

        public override void VisitParameterList(ParameterListSyntax node)
        {
            string name = "(" + string.Join(",", node.Parameters.Select(x => x.Type.ToString())) + ")";
            CSharpSyntaxMatchingNode initialNode = this.Root;

            this.Root = this.AddToNode(this.Root, name);
            this.Root.MatchingSyntaxNodes.Add(node.Parent);

            base.VisitParameterList(node);
            this.CopyTo(this.InvariantRoot, this.Root, name);
            this.Root = initialNode;
        }

        private void CopyTo(CSharpSyntaxMatchingNode root, CSharpSyntaxMatchingNode node, string name)
        {
            CSharpSyntaxMatchingNode n = this.AddToNode(root, name);
            int[] index = n.MatchingSyntaxNodes.Select(x => x.Span.Start).ToArray();
            n.MatchingSyntaxNodes.AddRange(node.MatchingSyntaxNodes.Where(x => !index.Contains(x.Span.Start)));
            if (node.Children.Count > 0)
            {
                foreach (string k in node.Children.Keys)
                {
                    this.CopyTo(n, node.Children[k], k);
                }
            }
        }

        private CSharpSyntaxMatchingNode AddToNode(CSharpSyntaxMatchingNode node, string name)
        {
            CSharpSyntaxMatchingNode firstLevelNode;
            if (node.Children.TryGetValue(name, out firstLevelNode))
            {
                return firstLevelNode;
            }
            else
            {
                firstLevelNode = new CSharpSyntaxMatchingNode();

                node.Children[name] = firstLevelNode;
                return firstLevelNode;
            }
            return firstLevelNode;
        }
    }
}