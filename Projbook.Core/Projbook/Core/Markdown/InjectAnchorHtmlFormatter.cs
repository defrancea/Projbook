using CommonMark;
using CommonMark.Formatters;
using CommonMark.Syntax;
using EnsureThat;
using Projbook.Core.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Projbook.Core.Markdown
{
    /// <summary>
    /// Implements markdown formatter for CommonMark.Net that inject anchor before each header.
    /// During the injection the formatter collect injected anchor in order in order to be able to generate a summary later.
    /// </summary>
    public class InjectAnchorHtmlFormatter : HtmlFormatter
    {
        /// <summary>
        /// Simple string identifying the formatting context.
        /// This value is used as a prefix when generating anchor name.
        /// Each formatter execution have to contain different context name in order to prevent conflicts.
        /// </summary>
        public string ContextName { get; private set; }

        /// <summary>
        /// Anchor array resulting of the injection processing.
        /// Each anchor correspond to a markdown header that is usable as hyperlink target.
        /// </summary>
        public Anchor[] Anchors { get { return anchors.Values.ToArray(); } }
        private Dictionary<string, Anchor> anchors = new Dictionary<string, Anchor>();

        /// <summary>
        /// Internal dictionary to resolve anchor conflicts.
        /// In case of no conflict the dictionary will remaings empty, however any conflict will create a slot with an integer representing the next available index.
        /// Everytime we meet a conflict, the anchor generation will increment the index and use it as suffix.
        /// </summary>
        private Dictionary<string, int> anchorConflicts = new Dictionary<string, int>();

        /// <summary>
        /// Initializes a new instance of <see cref="InjectAnchorHtmlFormatter"/>.
        /// </summary>
        /// <param name="contextName">Initializes the required <see cref="ContextName"/></param>
        /// <param name="target">Initializes the required text writter used as output.</param>
        /// <param name="settings">Initializes the required common mark settings used by the formatting.</param>
        public InjectAnchorHtmlFormatter(string contextName, TextWriter target, CommonMarkSettings settings)
            : base(target, settings)
        {
            // Data validation
            Ensure.That(() => contextName).IsNotNullOrWhiteSpace();

            // Initialize
            this.ContextName = contextName;
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
                string anchor = string.Format("{0}-{1}", this.ContextName, headerContent).ToLower();

                // Detect anchor conflict
                if (anchors.ContainsKey(anchor))
                {
                    // Compute an index to resolve conflicts
                    int index;
                    if (anchorConflicts.ContainsKey(anchor))
                    {
                        index = ++anchorConflicts[anchor];
                    }
                    else
                    {
                        index = anchorConflicts[anchor] = 2;
                    }

                    // Append the index
                    anchor = string.Format("{0}-{1}", anchor, index);
                }

                // Encode for url usage
                anchor = HttpUtility.UrlEncode(anchor);

                // Write anchor
                this.Write(string.Format(@"<a name=""{0}""></a>", anchor));

                // Keep track of the created anchor
                anchors[anchor] = new Anchor(
                    label: headerContent,
                    level: block.HeaderLevel,
                    value: anchor);
            }

            // Trigger parent rendering for the default html rendering
            base.WriteBlock(block, isOpening, isClosing, out ignoreChildNodes);
        }
    }
}