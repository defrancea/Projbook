using Markdig.Renderers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Markdig.Renderers.Html;

namespace Projbook.Core.Markdown
{
    public class ProjbookHtmlRenderer : HtmlRenderer
    {
        public ProjbookHtmlRenderer(TextWriter writer) : base(writer)
        {
            ObjectRenderers.Remove(ObjectRenderers.OfType<CodeBlockRenderer>().First());
            ObjectRenderers.Add(new ProjbookSnippetRenderer());
        }
    }
}