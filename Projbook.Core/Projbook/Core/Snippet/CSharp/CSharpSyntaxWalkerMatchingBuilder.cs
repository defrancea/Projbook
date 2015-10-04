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
                this.Root = this.Root.AddToNode(n);
                if (null == startNsNode)
                {
                    startNsNode = this.Root;
                }
            }
            this.Root.AddSyntaxNode(node);

            base.VisitNamespaceDeclaration(node);

            this.InvariantRoot.CopyTo(startNsNode, ns[0]);
            this.Root = initialNode;
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            string name = node.Identifier.ValueText;
            CSharpSyntaxMatchingNode initialNode = this.Root;

            this.Root = this.Root.AddToNode(name);
            this.Root.AddSyntaxNode(node);

            base.VisitClassDeclaration(node);
            this.InvariantRoot.CopyTo(this.Root, name);
            this.Root = initialNode;
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            string name = node.Identifier.ValueText;
            CSharpSyntaxMatchingNode initialNode = this.Root;

            this.Root = this.Root.AddToNode(name);
            this.Root.AddSyntaxNode(node);

            base.VisitPropertyDeclaration(node);
            this.InvariantRoot.CopyTo(this.Root, name);
            this.Root = initialNode;
        }

        public override void VisitAccessorDeclaration(AccessorDeclarationSyntax node)
        {
            string name = node.Keyword.ValueText;
            CSharpSyntaxMatchingNode initialNode = this.Root;

            this.Root = this.Root.AddToNode(name);
            this.Root.AddSyntaxNode(node);

            base.VisitAccessorDeclaration(node);
            this.InvariantRoot.CopyTo(this.Root, name);
            this.Root = initialNode;
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            string name = node.Identifier.ValueText;
            CSharpSyntaxMatchingNode initialNode = this.Root;

            this.Root = this.Root.AddToNode(name);
            this.Root.AddSyntaxNode(node);

            base.VisitMethodDeclaration(node);
            this.InvariantRoot.CopyTo(this.Root, name);
            this.Root = initialNode;
        }

        public override void VisitParameterList(ParameterListSyntax node)
        {
            string name = "(" + string.Join(",", node.Parameters.Select(x => x.Type.ToString())) + ")";
            CSharpSyntaxMatchingNode initialNode = this.Root;

            this.Root = this.Root.AddToNode(name);
            this.Root.AddSyntaxNode(node.Parent);

            base.VisitParameterList(node);
            this.InvariantRoot.CopyTo(this.Root, name);
            this.Root = initialNode;
        }
    }
}