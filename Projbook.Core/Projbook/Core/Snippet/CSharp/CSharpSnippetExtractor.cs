using EnsureThat;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Projbook.Core.Projbook.Core.Snippet.CSharp;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Projbook.Core.Snippet.CSharp
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
        public Model.Snippet Extract()
        {
            CSharpMatchingRule rule = CSharpMatchingRule.Parse(this.Pattern);

            // Load the file content
            FileInfo fileInfo = new FileInfo(Path.Combine(this.SourceDictionaries[0].FullName, rule.TargetFile)); // Todo: More validation and class member parsin with Roslyn
            MemoryStream memoryStream = new MemoryStream();
            using (var fileReader = new StreamReader(new FileStream(fileInfo.FullName, FileMode.Open)))
            using (var fileWriter = new StreamWriter(memoryStream))
            {
                fileWriter.Write(fileReader.ReadToEnd());
            }
            
            string code = Encoding.UTF8.GetString(memoryStream.ToArray());

            // Return the entire code if no member is specified
            if (rule.MatchingChunks.Length <= 0)
            {
                return new Model.Snippet(code);
            }
            
            SyntaxTree tree = CSharpSyntaxTree.ParseText(code);

            SyntaxNode root = tree.GetRoot();

            CSharpSyntaxWalkerMatchingBuilder trieBuilder = new CSharpSyntaxWalkerMatchingBuilder();
            trieBuilder.Visit(root);
            
            string t = trieBuilder.Root.ToString();
            FileInfo fi = new FileInfo("TrieOutput.txt");
            string fullPath = fi.FullName;
            using (var writer = new StreamWriter(new FileStream(fi.FullName, FileMode.Create)))
            {
                writer.Write(t);
            }

            CSharpSyntaxMatchingNode node = trieBuilder.Root.Match(rule.MatchingChunks);
            
            return this.BuildSnippet(node.MatchingSyntaxNodes);
        }

        private Model.Snippet BuildSnippet(SyntaxNode[] nodes)
        {
            // Todo implements many nodes
            SyntaxNode node = nodes[0];
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
            
            return new Model.Snippet(sb.ToString());
        }
    }
}