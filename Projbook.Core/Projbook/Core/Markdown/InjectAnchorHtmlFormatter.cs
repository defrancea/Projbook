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
        /// Identifier used as injected value for splitting page section.
        /// </summary>
        public string PageSlippingIdentifier { get; private set; }

        /// <summary>
        /// Internal dictionary to resolve section conflicts.
        /// In case of no conflict the dictionary will remaings empty, however any conflict will create a slot with an integer representing the next available index.
        /// Everytime we meet a conflict, the section generation will increment the index and use it as suffix.
        /// </summary>
        private Dictionary<string, int> sectionConflict = new Dictionary<string, int>();

        /// <summary>
        /// Initializes a new instance of <see cref="InjectAnchorHtmlFormatter"/>.
        /// </summary>
        /// <param name="contextName">Initializes the required <see cref="ContextName"/></param>
        /// <param name="pageSlippingIdentifier">Initializes the required <see cref="PageSlippingIdentifier"/></param>
        /// <param name="target">Initializes the required text writter used as output.</param>
        /// <param name="settings">Initializes the required common mark settings used by the formatting.</param>
        public InjectAnchorHtmlFormatter(string contextName, string pageSlippingIdentifier, TextWriter target, CommonMarkSettings settings)
            : base(target, settings)
        {
            // Data validation
            Ensure.That(() => contextName).IsNotNullOrWhiteSpace();
            Ensure.That(() => pageSlippingIdentifier).IsNotNullOrWhiteSpace();

            // Initialize
            this.ContextName = contextName;
            this.PageSlippingIdentifier = pageSlippingIdentifier;
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
                string sectionId = HttpUtility.UrlEncode(headerContent.ToLower());
                sectionId = string.Format("{0}.{1}", this.ContextName, sectionId);

                // Detect anchor conflict
                if (sectionConflict.ContainsKey(sectionId))
                {
                    // Append the index
                    sectionId = string.Format("{0}-{1}", sectionId, ++sectionConflict[sectionId]);
                }

                // Write anchor
                this.Write(string.Format(@"<!--{0} [{1}]({2})-->", this.PageSlippingIdentifier, headerContent, sectionId));
                sectionConflict[sectionId] = 1;
            }

            // Trigger parent rendering for the default html rendering
            base.WriteBlock(block, isOpening, isClosing, out ignoreChildNodes);
        }
    }
}