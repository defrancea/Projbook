using CommonMark;
using CommonMark.Formatters;
using CommonMark.Syntax;
using EnsureThat;
using Projbook.Extension.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Projbook.Core.Markdown
{
    /// <summary>
    /// Implements markdown formatter for CommonMark.Net that customize rendering for Projbook integration.
    /// </summary>
    public class ProjbookHtmlFormatter : HtmlFormatter
    {
        /// <summary>
        /// Simple string identifying the formatting context.
        /// This value is used as a prefix when generating anchor name.
        /// Each formatter execution have to contain different context name in order to prevent conflicts.
        /// </summary>
        public string ContextName { get; private set; }

        /// <summary>
        /// The page break info exposed as array to the outside.
        /// </summary>
        public PageBreakInfo[] PageBreak { get { return this.pageBreak.ToArray(); } }

        /// <summary>
        /// Internal page break list where page breaks are added during the processing.
        /// </summary>
        private List<PageBreakInfo> pageBreak;

        /// <summary>
        /// Internal dictionary to resolve section conflicts.
        /// In case of no conflict the dictionary will remain empty, however any conflict will create a slot with an integer representing the next available index.
        /// Every time we meet a conflict, the section generation will increment the index and use it as suffix.
        /// </summary>
        private Dictionary<string, int> sectionConflict = new Dictionary<string, int>();

        /// <summary>
        /// Table delimiter using dashes as separator.
        /// </summary>
        private static Regex dashDelimiter = new Regex("^(:?)-+(:?)$", RegexOptions.Compiled);

        /// <summary>
        /// Invalid title chars.
        /// </summary>
        private static Regex invalidTitleChars = new Regex("[^a-zA-Z0-9]", RegexOptions.Compiled);

        /// <summary>
        /// The stream writer used during document generation.
        /// </summary>
        private StreamWriter writer;

        /// <summary>
        /// The section title base.
        /// </summary>
        private int sectionTitleBase;

        /// <summary>
        /// The snippet dictionary containing all snippets associated with guids.
        /// </summary>
        private Dictionary<Guid, Extension.Model.Snippet> snippetDictionary;

        /// <summary>
        /// The snippet reference prefix.
        /// </summary>
        private string snippetReferencePrefix;

        /// <summary>
        /// Initializes a new instance of <see cref="ProjbookHtmlFormatter"/>.
        /// </summary>
        /// <param name="contextName">Initializes the required <see cref="ContextName"/></param>
        /// <param name="target">Initializes the required text writer used as output.</param>
        /// <param name="settings">Initializes the required common mark settings used by the formatting.</param>
        /// <param name="sectionTitleBase">Initializes the section title base.</param>
        /// <param name="snippetDictionary">Initializes the snippet directory.</param>
        /// <param name="snippetReferencePrefix">Initializes the snippet reference prefix.</param>
        public ProjbookHtmlFormatter(string contextName, TextWriter target, CommonMarkSettings settings, int sectionTitleBase, Dictionary<Guid, Extension.Model.Snippet> snippetDictionary, string snippetReferencePrefix)
            : base(target, settings)
        {
            // Data validation
            Ensure.That(() => contextName).IsNotNullOrWhiteSpace();
            Ensure.That(target is StreamWriter).IsTrue();
            Ensure.That(() => sectionTitleBase).IsGte(0);
            Ensure.That(() => snippetDictionary).IsNotNull();
            Ensure.That(() => snippetReferencePrefix).IsNotNull();

            // Initialize
            this.ContextName = contextName;
            this.pageBreak = new List<PageBreakInfo>();
            this.writer = target as StreamWriter;
            this.sectionTitleBase = sectionTitleBase;
            this.snippetDictionary = snippetDictionary;
            this.snippetReferencePrefix = snippetReferencePrefix;
        }

        /// <summary>
        /// Specializes block writing for anchor injection.
        /// For each formatted header we generate an anchor based on the context name, the header content and eventually add an integer suffix in order to prevent conflicts.
        /// </summary>
        /// <param name="block">The block to process.</param>
        /// <param name="isOpening">Define whether the block is opening.</param>
        /// <param name="isClosing">Defines whether the block is closing.</param>
        /// <param name="ignoreChildNodes">return whether the processing ignored child nodes.</param>
        protected override void WriteBlock(Block block, bool isOpening, bool isClosing, out bool ignoreChildNodes)
        {
            // Process block content
            if (null != block.StringContent)
            {
                // Read current block content
                string content = block.StringContent.TakeFromStart(block.StringContent.Length);

                // Detect snippet reference
                if (content.StartsWith(snippetReferencePrefix))
                {
                    // Fetch matching snippet
                    Extension.Model.Snippet snippet = snippetDictionary[Guid.Parse(content.Substring(snippetReferencePrefix.Length))];

                    // Render and write plain text snippet
                    PlainTextSnippet plainTextSnippet = snippet as PlainTextSnippet;
                    if (null != plainTextSnippet)
                    {
                        block.StringContent.Replace(plainTextSnippet.Text, 0, plainTextSnippet.Text.Length);
                    }

                    // Render and write node snippet
                    NodeSnippet nodeSnippet = snippet as NodeSnippet;
                    if (null != nodeSnippet)
                    {
                        // Render node as html
                        string renderedNode = Render(nodeSnippet.Node);

                        // Write rendering
                        block.StringContent.Replace(renderedNode, 0, renderedNode.Length);
                    }
                }
            }

            // Filter opening header
            if (isOpening && null != block && block.Tag == BlockTag.AtxHeading)
            {
                // Apply section title base
                block.Heading = new HeadingData(block.Heading.Level + this.sectionTitleBase);

                // Retrieve header content
                string headerContent;
                if (null != block.InlineContent && null != block.InlineContent.LiteralContent)
                {
                    // Read the whole content
                    Inline inline = block.InlineContent;
                    StringBuilder stringBuilder = new StringBuilder();
                    do
                    {
                        stringBuilder.Append(inline.LiteralContent);
                        inline = inline.NextSibling;
                    } while (null != inline);
                    headerContent = stringBuilder.ToString();
                }
                else
                {
                    headerContent = "unknown";
                }

                // Compute the anchor value
                string sectionId = headerContent.ToLower();
                sectionId = invalidTitleChars.Replace(string.Format("{0}-{1}", this.ContextName, sectionId), "-");

                // Detect anchor conflict
                if (sectionConflict.ContainsKey(sectionId))
                {
                    // Append the index
                    sectionId = string.Format("{0}-{1}", sectionId, ++sectionConflict[sectionId]);
                }

                // Flush the writer to move the stream position used during page break creation
                this.writer.Flush();

                // Add a new page break
                this.pageBreak.Add(new PageBreakInfo(sectionId, Math.Max(0, (int)block.Heading.Level), headerContent, this.writer.BaseStream.Position));

                // Initialize section conflict
                sectionConflict[sectionId] = 1;
            }

            // Read all paragraph inline strings in order to make table detectable
            List<string> tableParts = new List<string>();
            List<string[]> splittedTableParts = new List<string[]>();
            Match[] headerDelimiterMatches = null;
            if (isOpening && null != block && block.Tag == BlockTag.Paragraph)
            {
                Inline inline = block.InlineContent;
                while (null != inline)
                {
                    if (inline.Tag == InlineTag.String)
                    {
                        // Read and split line on '|'
                        tableParts.Add(inline.LiteralContent);
                        string line = inline.LiteralContent.Trim();
                        string[] lineParts = line.Split(new char[] { '|' }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();

                        // At the third line of the same block, ensure we're processing a table before to continue table parsing
                        if (splittedTableParts.Count == 2)
                        {
                            // If the second line cannot be a header delimiter, break the table parsing
                            if (splittedTableParts[1].Any(x => string.IsNullOrWhiteSpace(x)) || (splittedTableParts[1].Length < 2 && !tableParts[1].Contains('|')))
                            {
                                break;
                            }
                            
                            // Matches the second line as header delimiter and abort table parsing if the match fail
                            headerDelimiterMatches = splittedTableParts[1].Select(x => dashDelimiter.Match(x)).ToArray();
                            if (!headerDelimiterMatches.All(x => x.Success))
                            {
                                break;
                            }
                        }

                        // Define parts boundaries
                        int startPos = 0;
                        int numbber = lineParts.Length;

                        // Ignore the first part if the line starts with '|'
                        if ('|' == line.First())
                        {
                            ++startPos;
                            --numbber;
                        }
                        
                        // Ignore the last part if the line ends with '|'
                        if ('|' == line.Last())
                        {
                            --numbber;
                        }

                        // Add the line to splitted parts
                        splittedTableParts.Add(lineParts.Skip(startPos).Take(numbber).ToArray());
                    }
                    inline = inline.NextSibling;
                }
            }
            
            // Process table rendering
            if (null != headerDelimiterMatches && splittedTableParts.Count > 2)
            {
                // Remove the delimiter
                splittedTableParts.RemoveAt(1);

                // Render table
                this.Write(@"<table class=""table"">");
                for (int i = 0; i < splittedTableParts.Count; ++i)
                {
                    // Render rows
                    this.Write("<tr>");

                    // Render cells
                    for (int j = 0; j < splittedTableParts[i].Length; ++j)
                    {
                        // Determine text alignment
                        string style = "text-left";
                        if (headerDelimiterMatches.Length > j && 0 < i)
                        {
                            Match match = headerDelimiterMatches[j];
                            if (":" == match.Groups[1].Value && ":" == match.Groups[2].Value)
                            {
                                style = "text-center";
                            }
                            else if ("" == match.Groups[1].Value && ":" == match.Groups[2].Value)
                            {
                                style = "text-right";
                            }
                        }

                        // Generate markup
                        string tag = 0 == i ? "th" : "td";
                        this.Write(string.Format(@"<{0} class=""{1}"">", tag, style));
                        this.Write(splittedTableParts[i][j]);
                        this.Write(string.Format("</{0}>", tag));
                    }

                    this.Write("</tr>");
                }
                this.Write("</table>");

                // Report rendering finished
                ignoreChildNodes = true;
                return;
            }
            
            // Trigger parent rendering for the default html rendering
            base.WriteBlock(block, isOpening, isClosing, out ignoreChildNodes);
        }
        
        /// <summary>
        /// Renders <see cref="Node"/>.
        /// </summary>
        /// <param name="node">The node to render.</param>
        private string Render(Node node)
        {
            // Data validation
            Ensure.That(() => node).IsNotNull();

            // Initialize string builder for rendering
            StringBuilder stringBuilder = new StringBuilder();

            // Render tree
            stringBuilder.Append(@"<div class=""filetree"">");
            Render(node, stringBuilder);
            stringBuilder.Append("</div>");

            // Return built string
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Renders <see cref="Node"/>.
        /// </summary>
        /// <param name="node">The node to render.</param>
        /// <param name="stringBuilder">The string builder used as rendering output.</param>
        private void Render(Node node, StringBuilder stringBuilder)
        {
            // Data validation
            Ensure.That(() => node).IsNotNull();
            Ensure.That(() => stringBuilder).IsNotNull();

            // Render node opening
            stringBuilder.Append("<ul>");
            stringBuilder.Append(@"<li data-jstree='{""type"":""");
            stringBuilder.Append(node.IsLeaf ? "file" : "folder");
            stringBuilder.Append(@"""}'>");
            stringBuilder.Append(node.Name);

            // Recurse for children
            foreach (Node currentNode in node.Children.OrderBy(x => x.Key).Select(x => x.Value))
            {
                Render(currentNode, stringBuilder);
            }

            // Render node closing
            stringBuilder.Append("</li>");
            stringBuilder.Append("</ul>");
        }
    }
}