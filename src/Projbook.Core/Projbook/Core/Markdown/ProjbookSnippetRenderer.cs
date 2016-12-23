using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Projbook.Extension.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projbook.Core.Markdown
{
    public class ProjbookSnippetRenderer : CodeBlockRenderer
    {
        protected override void Write(HtmlRenderer renderer, CodeBlock obj)
        {
            NodeSnippet ns = obj.GetData(ProjbookEngine.SNIPPET_REFERENCE_PREFIX) as NodeSnippet;
            if (null != ns)
            {

                Render(ns.Node, renderer);
                return;
            }

            PlainTextSnippet s = obj.GetData(ProjbookEngine.SNIPPET_REFERENCE_PREFIX) as PlainTextSnippet;
            
                RenderCode(renderer, obj, s?.Text);
            
        }

        private void RenderCode(HtmlRenderer renderer, CodeBlock obj, string text)
        {

            renderer.EnsureLine();

            var fencedCodeBlock = obj as FencedCodeBlock;
            if (fencedCodeBlock?.Info != null && BlocksAsDiv.Contains(fencedCodeBlock.Info))
            {
                var infoPrefix = (obj.Parser as FencedCodeBlockParser)?.InfoPrefix ??
                                 FencedCodeBlockParser.DefaultInfoPrefix;

                // We are replacing the HTML attribute `language-mylang` by `mylang` only for a div block
                // NOTE that we are allocating a closure here
                renderer.Write("<div")
                    .WriteAttributes(obj.TryGetAttributes(),
                        cls => cls.StartsWith(infoPrefix) ? cls.Substring(infoPrefix.Length) : cls)
                    .Write(">");
                if (null != text)
                    renderer.Write(text);
                else
                    renderer.WriteLeafRawLines(obj, true, true, true);
                renderer.WriteLine("</div>");

            }
            else
            {
                renderer.Write("<pre");
                if (OutputAttributesOnPre)
                {
                    renderer.WriteAttributes(obj);
                }
                renderer.Write("><code");
                if (!OutputAttributesOnPre)
                {
                    renderer.WriteAttributes(obj);
                }
                renderer.Write(">");
                if (null != text)
                    renderer.Write(text);
                else
                    renderer.WriteLeafRawLines(obj, true, true, true);
                renderer.WriteLine("</code></pre>");
            }
        }



        /// <summary>
        /// Renders <see cref="Node"/>.
        /// </summary>
        /// <param name="node">The node to render.</param>
        private string Render(Node node, HtmlRenderer renderer)
        {
            // Data validation
            //Ensure.That(() => node).IsNotNull();

            // Initialize string builder for rendering
            StringBuilder stringBuilder = new StringBuilder();

            // Render tree
            renderer.Write(@"<div class=""filetree"">");
            Render2(node, renderer);
            renderer.Write("</div>");

            // Return built string
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Renders <see cref="Node"/>.
        /// </summary>
        /// <param name="node">The node to render.</param>
        /// <param name="stringBuilder">The string builder used as rendering output.</param>
        private void Render2(Node node, HtmlRenderer renderer)
        {
            // Data validation
            //Ensure.That(() => node).IsNotNull();
            //Ensure.That(() => stringBuilder).IsNotNull();

            // Render node opening
            renderer.Write("<ul>");
            renderer.Write(@"<li data-jstree='{""type"":""");
            renderer.Write(node.IsLeaf ? "file" : "folder");
            renderer.Write(@"""}'>");
            renderer.Write(node.Name);

            // Recurse for children
            foreach (Node currentNode in node.Children.OrderBy(x => x.Value.IsLeaf).ThenBy(x => x.Key).Select(x => x.Value))
            {
                Render2(currentNode, renderer);
            }

            // Render node closing
            renderer.Write("</li>");
            renderer.Write("</ul>");
        }
    }
}