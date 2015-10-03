using EnsureThat;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Projbook.Core.Projbook.Core.Model;
using Projbook.Core.Projbook.Core.Snippet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Projbook.Core.Snippet
{
    /// <summary>
    /// Extractor in charge of browsing source directories. load file content and extract requested member.
    /// </summary>
    public class CSharpSnippetExtractor : ISnippetExtractor
    {
        /// <summary>
        /// All source directories where snippets could possibly be.
        /// </summary>
        public DirectoryInfo[] SourceDictionaries { get; private set; }

        /// <summary>
        /// Snippet extraction pattern.
        /// </summary>
        public string Pattern { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="CSharpSnippetExtractor"/>.
        /// </summary>
        /// <param name="pattern">Initializes the required <see cref="Pattern"/>.</param>
        /// <param name="sourceDirectories">Initializes the required <see cref="SourceDictionaries"/>.</param>
        public CSharpSnippetExtractor(string pattern, params DirectoryInfo[] sourceDirectories)
        {
            // Data validation
            Ensure.That(() => pattern).IsNotNullOrWhiteSpace();
            Ensure.That(() => sourceDirectories).IsNotNull();
            Ensure.That(() => sourceDirectories).HasItems();

            // Initialize
            this.Pattern = pattern;
            this.SourceDictionaries = sourceDirectories;
        }

        /// <summary>
        /// Extracts a snippet from a given rule pattern.
        /// </summary>
        /// <param name="rule">The rule to parse and extract snippet from.</param>
        /// <returns>The extracted snippet.</returns>
        public Model.Razor.Snippet Extract()
        {
            //Regex regex = new Regex(@"^\s*([^\s]+)(\s+([^(\s]+)\s*(\(([^)]*\s*)\))?)?\s*$", RegexOptions.Compiled);
            //Match match = regex.Match(this.Pattern);

            //Match match1 = regex.Match("A/Foo.txt");
            //Match match2 = regex.Match("A/Foo.txt Foo.Bar");
            //Match match3 = regex.Match("A/Foo.txt Foo.Bar   ()");
            //Match match4 = regex.Match("A/Foo.txt Foo.Bar   (string,int)");
            SnippetMatchingRule rule1 = SnippetMatchingRule.Parse("A/Foo.txt");
            SnippetMatchingRule rule2 = SnippetMatchingRule.Parse("A/Foo.txt Foo.Bar");
            SnippetMatchingRule rule3 = SnippetMatchingRule.Parse("A/Foo.txt Foo.Bar   ()");
            SnippetMatchingRule rule4 = SnippetMatchingRule.Parse("A/Foo.txt Foo.Bar   (string,int)");


            SnippetMatchingRule rule = SnippetMatchingRule.Parse(this.Pattern);

            //string file = match.Groups[1].Value;
            //string rawMember = match.Groups[2].Value;
            //string rawParameters = match.Groups[3].Value;
            
            //string[] memberChunk = rawMember.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            // check lenght >= 2

            //string ns = string.Join(".", memberChunk.Take(memberChunk.Length - 2));
            //string clazz = memberChunk[memberChunk.Length - 2];
            //string member = memberChunk[memberChunk.Length - 1];

            //string[] parameters = rawParameters.Split(new char[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

            // Load the file content
            FileInfo fileInfo = new FileInfo(Path.Combine(this.SourceDictionaries[0].FullName, rule.File)); // Todo: More validation and class member parsin with Roslyn
            MemoryStream memoryStream = new MemoryStream();
            using (var fileReader = new StreamReader(new FileStream(fileInfo.FullName, FileMode.Open)))
            using (var fileWriter = new StreamWriter(memoryStream))
            {
                fileWriter.Write(fileReader.ReadToEnd());
            }

            //string member = match.Groups[2].Value.Replace(" ", string.Empty);
            string code = Encoding.UTF8.GetString(memoryStream.ToArray());

            SyntaxTree tree = CSharpSyntaxTree.ParseText(code);

            SyntaxNode root = tree.GetRoot();
            /*NamespaceDeclarationSyntax[] s = tree.GetRoot().DescendantNodes().OfType<NamespaceDeclarationSyntax>().Where(x => x.Name.ToString() == ns).ToArray();

            ClassDeclarationSyntax[] cs = s[0].Members.OfType<ClassDeclarationSyntax>().Where(x => x.Identifier.ValueText == clazz).ToArray();

            MethodDeclarationSyntax[] ms = cs[0].Members.OfType<MethodDeclarationSyntax>().Where(x => x.Identifier.ValueText == member).ToArray();
            
            var compilation = CSharpCompilation.Create("HelloWorld")
                                               .AddReferences(
                                                    MetadataReference.CreateFromFile(
                                                        typeof(object).Assembly.Location))
                                               .AddSyntaxTrees(tree);

            var model = compilation.GetSemanticModel(tree);

            var nameInfo = model.GetSymbolInfo(((CompilationUnitSyntax)tree.GetRoot()).Usings[0].Name);

            var systemSymbol = (INamespaceSymbol)nameInfo.Symbol;

            foreach (var ns2 in systemSymbol.GetNamespaceMembers())
            {
                Console.WriteLine(ns2.Name);
            }*/

            /*foreach (SyntaxNode node in root.ChildNodes())
            {
                NamespaceDeclarationSyntax namespaceSyntax = node as NamespaceDeclarationSyntax;
                if (null != namespaceSyntax)
                {

                }
            }*/

            TrieBuilderVisiter trieBuilder = new TrieBuilderVisiter();
            trieBuilder.Visit(root);

            StringBuilder s = new StringBuilder();
            this.Print(s, "", trieBuilder.Root, 0);
            FileInfo fi = new FileInfo("TrieOutput.txt");
            string fullPath = fi.FullName;
            using (var writer = new StreamWriter(new FileStream(fi.FullName, FileMode.Create)))
            {
                writer.Write(s.ToString());
            }

            /*Node n1 = trieBuilder.Root.Nodes["A"];
            Node n2 = trieBuilder.Root.Nodes["NS"].Nodes["OneLevelNamespaceClass"].Nodes["SubClass"];
            Node n3 = trieBuilder.Root.Nodes["OneLevelNamespaceClass"].Nodes["SubClass"];
            Node n4 = trieBuilder.Root.Nodes["NS2"].Nodes["NS2"].Nodes["NS3"].Nodes["A"];*/



            // Return the entire code if no member is specified
            if (rule.MemberChunks.Length == 0)
            {
                return new Model.Razor.Snippet(code);
            }

            // If member chunks are specified but is not a method, We need to match two possible cases:
            // 1. Namespace
            // 2. Class
            // For performence reason the algorithm is the following:
            // 1. Match all namespace based on n-1 chunk
            // 2. Match the last member
            /*if (rule.MemberChunks.Length > 0 && !rule.IsMethod)
            {
                if (rule.MemberChunks.Length == 1)
                {
                    var nsDeclaration = root
                       .DescendantNodes()
                       .OfType<NamespaceDeclarationSyntax>()
                       .FirstOrDefault(x => x.Name.ToString() == rule.MemberChunks[0]);
                    if (null != nsDeclaration)
                    {
                        return this.BuildSnippet(nsDeclaration);
                    }
                    
                    var classDeclaration = root
                       .DescendantNodes()
                       .OfType<ClassDeclarationSyntax>()
                       .FirstOrDefault(x => x.Identifier.ValueText == rule.MemberChunks[0]);
                    if (null != classDeclaration)
                    {
                        return this.BuildSnippet(classDeclaration);
                    }
                }

                // For performence purpose, matche the n-1 member only
                string nsToMatch = string.Join(".", rule.MemberChunks.Take(rule.MemberChunks.Length - 1));
                var prefixDeclaration = root
                    .DescendantNodes()
                    .OfType<NamespaceDeclarationSyntax>()
                    .FirstOrDefault(x => x.Name.ToString().StartsWith(nsToMatch));
                if (null != prefixDeclaration)
                {
                    var prefixClassDeclaration = root
                        .DescendantNodes()
                        .OfType<ClassDeclarationSyntax>()
                        .FirstOrDefault(x => x.Identifier.ValueText == rule.MemberChunks.Last());
                    if (null != prefixClassDeclaration)
                    {
                        return this.BuildSnippet(prefixClassDeclaration);
                    }
                }

                

            }*/

            return null;
        }

        private Model.Razor.Snippet BuildSnippet(SyntaxNode node)
        {
            string[] lines = node.GetText().Lines.Select(x => x.ToString()).ToArray();

            int start = 0;
            for (; start < lines.Length && lines[start].ToString().Trim().Length == 0; ++start) ;

            int end = lines.Length - 1;
            for (; 0 <= end && lines[end].ToString().Trim().Length == 0; --end) ;

            int pad = int.MaxValue;
            for (int i = start; i <= end; ++i)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                {
                    pad = Math.Min(pad, lines[i].ToString().TakeWhile(Char.IsWhiteSpace).Count());
                }
            }

            StringBuilder sb = new StringBuilder();
            bool needNewLine = false;
            for (int i = start; i <= end; ++i)
            {
                if (needNewLine)
                {
                    sb.AppendLine();
                }

                if (lines[i].Length > pad)
                {
                    sb.Append(lines[i].Substring(pad));
                }
                needNewLine = true;
            }
            
            return new Model.Razor.Snippet(sb.ToString());
        }

        void Print(StringBuilder sb, string name, Node node, int i)
        {
            sb.AppendLine(new string('-', i) + name);
            foreach (var k in node.SyntaxNodes)
            {
                sb.AppendLine(new string('-', i) +"[" + k.GetType().Name + "]");
            }
            foreach (string k in node.Nodes.Keys)
            {
                this.Print(sb, k, node.Nodes[k], 1 + i);
            }
        }

        class TrieBuilderVisiter : CSharpSyntaxWalker
        {
            public Node Root { get; set; }

            public Node InvariantRoot { get; set; }

            public TrieBuilderVisiter()
            {
                this.Root = new Node();
                this.InvariantRoot = this.Root;

            }

            public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
            {
                string name = node.Name.ToString();
                string[] ns = name.Split('.');
                Node initialNode = this.Root;

                Node startNsNode = null;
                
                foreach (string n in ns)
                {
                    this.Root = this.AddToNode(this.Root, n);
                    if (null == startNsNode)
                    {
                        startNsNode = this.Root;
                    }
                }
                this.Root.SyntaxNodes.Add(node);

                /*if (this.InvariantRoot != initialNode)
                {
                    this.AddToNode(this.InvariantRoot, ns[0], startNsNode);
                }*/

                base.VisitNamespaceDeclaration(node);
                this.CopyTo(this.InvariantRoot, startNsNode, ns[0]);
                this.Root = initialNode;
            }

            public override void VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                string name = node.Identifier.ValueText;
                Node initialNode = this.Root;

                this.Root = this.AddToNode(this.Root, name);
                this.Root.SyntaxNodes.Add(node);
                
                /*if (this.InvariantRoot != initialNode)
                {
                    this.AddToNode(this.InvariantRoot, name, this.Root);
                }*/

                base.VisitClassDeclaration(node);
                this.CopyTo(this.InvariantRoot, this.Root, name);
                this.Root = initialNode;
            }

            public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
            {
                string name = node.Identifier.ValueText;
                Node initialNode = this.Root;

                this.Root = this.AddToNode(this.Root, name);
                this.Root.SyntaxNodes.Add(node);

                base.VisitPropertyDeclaration(node);
                this.CopyTo(this.InvariantRoot, this.Root, name);
                this.Root = initialNode;
            }

            public override void VisitAccessorDeclaration(AccessorDeclarationSyntax node)
            {
                string name = node.Keyword.ValueText;
                Node initialNode = this.Root;

                this.Root = this.AddToNode(this.Root, name);
                this.Root.SyntaxNodes.Add(node);

                base.VisitAccessorDeclaration(node);
                this.CopyTo(this.InvariantRoot, this.Root, name);
                this.Root = initialNode;
            }

            public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                string name = node.Identifier.ValueText;
                Node initialNode = this.Root;

                this.Root = this.AddToNode(this.Root, name);
                this.Root.SyntaxNodes.Add(node);

                base.VisitMethodDeclaration(node);
                this.CopyTo(this.InvariantRoot, this.Root, name);
                this.Root = initialNode;
            }

            public override void VisitParameterList(ParameterListSyntax node)
            {
                string name = "(" + string.Join(",", node.Parameters.Select(x => x.Type.ToString())) + ")";
                Node initialNode = this.Root;

                this.Root = this.AddToNode(this.Root, name);
                this.Root.SyntaxNodes.Add(node.Parent);

                base.VisitParameterList(node);
                this.CopyTo(this.InvariantRoot, this.Root, name);
                this.Root = initialNode;
            }

            private void CopyTo(Node root, Node node, string name)
            {
                Node n = this.AddToNode(root, name);
                int[] index = n.SyntaxNodes.Select(x => x.Span.Start).ToArray();
                n.SyntaxNodes.AddRange(node.SyntaxNodes.Where(x => !index.Contains(x.Span.Start)));
                if (node.Nodes.Count > 0)
                {
                    foreach (string k in node.Nodes.Keys)
                    {
                        this.CopyTo(n, node.Nodes[k], k);
                    }
                }
            }

            private Node AddToNode(Node node, string name/*, Node newNode = null*/)
            {
                Node firstLevelNode;
                if (node.Nodes.TryGetValue(name, out firstLevelNode))
                {
                    // Add existing node children
                    /*if (null != newNode)
                    {
                        firstLevelNode.SyntaxNodes.AddRange(newNode.SyntaxNodes);
                    }*

                    // Recurse
                    /*if (null != newNode && newNode.Nodes.Count > 0)
                    {
                        foreach (string k in newNode.Nodes.Keys)
                        {
                            this.AddToNode(firstLevelNode, k, newNode.Nodes[k]);
                        }
                    }*/
                    return firstLevelNode;
                }
                else
                {
                    firstLevelNode = new Node();
                    /*if (null != newNode)
                    {
                        firstLevelNode.Nodes = newNode.Nodes;
                        firstLevelNode.SyntaxNodes = newNode.SyntaxNodes;
                    }*/

                    node.Nodes[name] = firstLevelNode;
                    return firstLevelNode;
                }
                return firstLevelNode;
            }
        }
    }
}