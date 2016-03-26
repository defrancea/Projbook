using CommonMark;
using CommonMark.Formatters;
using CommonMark.Syntax;
using EnsureThat;
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
        /// In case of no conflict the dictionary will remaings empty, however any conflict will create a slot with an integer representing the next available index.
        /// Everytime we meet a conflict, the section generation will increment the index and use it as suffix.
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
        /// Initializes a new instance of <see cref="ProjbookHtmlFormatter"/>.
        /// </summary>
        /// <param name="contextName">Initializes the required <see cref="ContextName"/></param>
        /// <param name="target">Initializes the required text writter used as output.</param>
        /// <param name="settings">Initializes the required common mark settings used by the formatting.</param>
        /// <param name="sectionTitleBase">Initializes the section title base.</param>
        public ProjbookHtmlFormatter(string contextName, TextWriter target, CommonMarkSettings settings, int sectionTitleBase)
            : base(target, settings)
        {
            // Data validation
            Ensure.That(() => contextName).IsNotNullOrWhiteSpace();
            Ensure.That(target is StreamWriter).IsTrue();
            Ensure.That(() => sectionTitleBase).IsGte(0);

            // Initialize
            this.ContextName = contextName;
            this.pageBreak = new List<PageBreakInfo>();
            this.writer = target as StreamWriter;
            this.sectionTitleBase = sectionTitleBase;
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
            // Filter opening header
            if (isOpening && null != block && block.Tag == BlockTag.AtxHeader)
            {
                // Apply section title base
                block.HeaderLevel += this.sectionTitleBase;

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
                this.pageBreak.Add(new PageBreakInfo(sectionId, headerContent, this.writer.BaseStream.Position));

                // Initialize section conflict
                sectionConflict[sectionId] = 1;
            }

            // Read all paragraph inline strings in order to make table detectable
            List<string> tableParts = new List<string>();
            List<string[]> splittedTableParts = new List<string[]>();
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
            
            // Detect tables
            Match[] matchingDelimiters = null;
            bool isTable = false;
            if (3 <= splittedTableParts.Count && splittedTableParts[1].All(x => !string.IsNullOrWhiteSpace(x)) && (splittedTableParts[1].Length >= 2 || tableParts[1].Contains('|')))
            {
                // Match dashes
                matchingDelimiters = splittedTableParts[1].Select(x => dashDelimiter.Match(x)).ToArray();
                isTable = matchingDelimiters.All(x => x.Success);
            }
            
            // Process table rendering
            if (isTable)
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
                        if (matchingDelimiters.Length > j && 0 < i)
                        {
                            Match match = matchingDelimiters[j];
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
    }
}