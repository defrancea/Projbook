using EnsureThat;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Projbook.Core.Projbook.Core.Snippet.CSharp;
using System;
using System.Collections.Generic;
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
        /// Expose the extractor language.
        /// </summary>
        public string Language { get { return "csharp"; } }

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
            // Parse the matching rule from the pattern
            CSharpMatchingRule rule = CSharpMatchingRule.Parse(this.Pattern);

            // Load the file content
            FileInfo fileInfo = new FileInfo(Path.Combine(this.SourceDictionaries[0].FullName, rule.TargetFile)); // Todo: More validation and class member parsin with Roslyn
            MemoryStream memoryStream = new MemoryStream();
            using (var fileReader = new StreamReader(new FileStream(fileInfo.FullName, FileMode.Open)))
            using (var fileWriter = new StreamWriter(memoryStream))
            {
                fileWriter.Write(fileReader.ReadToEnd());
            }
            
            // Read the code snippet from the file
            string sourceCode = Encoding.UTF8.GetString(memoryStream.ToArray());

            // Return the entire code if no member is specified
            if (rule.MatchingChunks.Length <= 0)
            {
                return this.BuildSnippet(sourceCode);
            }
            
            // Build a syntax tree from the source code
            SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceCode);
            SyntaxNode root = tree.GetRoot();

            // Visit the syntax tree for generating a Trie for pattern matching
            CSharpSyntaxWalkerMatchingBuilder syntaxMatchingBuilder = new CSharpSyntaxWalkerMatchingBuilder();
            syntaxMatchingBuilder.Visit(root);

            // Match the rule from the syntax matching Trie
            CSharpSyntaxMatchingNode node = syntaxMatchingBuilder.Root.Match(rule.MatchingChunks); // Todo: Raise error if the matching is null
            
            // Build a snippet for extracted syntax nodes
            return this.BuildSnippet(node.MatchingSyntaxNodes);
        }

        /// <summary>
        /// Builds a snippet from extracted syntax nodes.
        /// </summary>
        /// <param name="nodes">The exctracted nodes.</param>
        /// <returns>The built snippet.</returns>
        private Model.Snippet BuildSnippet(SyntaxNode[] nodes)
        {
            // Data validation
            Ensure.That(() => nodes).IsNotNull();
            Ensure.That(() => nodes).HasItems();

            // Extract code from each snippets
            StringBuilder stringBuilder = new StringBuilder();
            bool firstSnippet = true;
            foreach (SyntaxNode node in nodes)
            {
                // Write line return between each snippet
                if (!firstSnippet)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine();
                }

                // Write each snippet line
                string[] lines = node.GetText().Lines.Select(x => x.ToString()).ToArray();
                this.WriteAndCleanupSnippet(stringBuilder, lines);

                // Flag the first snippet as false
                firstSnippet = false;
            }
            
            // Create the snippet from the exctracted code
            return new Model.Snippet(stringBuilder.ToString());
        }

        /// <summary>
        /// Builds a snippet from a full file content.
        /// </summary>
        /// <param name="fileContent">The file content.</param>
        /// <returns>The built snippet.</returns>
        private Model.Snippet BuildSnippet(string fileContent)
        {
            // Data validation
            Ensure.That(() => fileContent).IsNotNull();

            // Extract each lines
            StringBuilder stringBuilder = new StringBuilder();
            List<string> lines = new List<string>();
            using (StringReader stringReader = new StringReader(fileContent))
            {
                while (true)
                {
                    string line = stringReader.ReadLine();
                    if (null != line)
                    {
                        lines.Add(line);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            this.WriteAndCleanupSnippet(stringBuilder, lines.ToArray());

            // Create the snippet from the exctracted code
            return new Model.Snippet(stringBuilder.ToString());
        }

        /// <summary>
        /// Writes and cleanup line snippets.
        /// Snippets are moved out of their context, for this reasong we need to trim lines aroung and remove a part of the indentation.
        /// </summary>
        /// <param name="stringBuilder">The string builder used as output.</param>
        /// <param name="lines">The lines to process.</param>
        private void WriteAndCleanupSnippet(StringBuilder stringBuilder, string[] lines)
        {
            // Data validation
            Ensure.That(() => stringBuilder).IsNotNull();
            Ensure.That(() => lines).IsNotNull();

            // Do not process if lines are empty
            if (0 >= lines.Length)
            {
                return;
            }

            // Compute the index of the first non empty line
            int startPos = 0;
            for (; startPos < lines.Length && lines[startPos].ToString().Trim().Length == 0; ++startPos) ;

            // Compute the index of the last non empty line
            int endPos = lines.Length - 1;
            for (; 0 <= endPos && lines[endPos].ToString().Trim().Length == 0; --endPos) ;

            // Compute the padding to remove for removing a part of the indentation
            int leftPadding = int.MaxValue;
            for (int i = startPos; i <= endPos; ++i)
            {
                // Ignore empty lines in the middle of the snippet
                if (!string.IsNullOrWhiteSpace(lines[i]))
                {
                    // Adjust the left padding with the available whilespace at the beginning of the line
                    leftPadding = Math.Min(leftPadding, lines[i].ToString().TakeWhile(Char.IsWhiteSpace).Count());
                }
            }

            // Write selected lines to the string builder
            bool firstLine = true;
            for (int i = startPos; i <= endPos; ++i)
            {
                // Write line return between each line
                if (!firstLine)
                {
                    stringBuilder.AppendLine();
                }

                // Remove a part of the indentation padding
                if (lines[i].Length > leftPadding)
                {
                    stringBuilder.Append(lines[i].Substring(leftPadding));
                }

                // Flag the first line as false
                firstLine = false;
            }
        }
    }
}