using Markdig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Projbook.Core.Markdown.Extensions
{
    public class ProjbookStuff : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            HtmlRenderer r = renderer as HtmlRenderer;
            if (null != r)
            {
                r.ObjectRenderers.Remove(r.ObjectRenderers.OfType<CodeBlockRenderer>().First());
                r.ObjectRenderers.Add(new ProjbookSnippetRenderer());
            }
        }
    }
}
