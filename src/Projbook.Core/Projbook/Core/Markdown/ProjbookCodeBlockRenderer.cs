using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
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

            if (0 == obj.Lines.Count && null != s && 0 < s.Text.Length)
            {
                var s2 = new Markdig.Helpers.StringSlice(s.Text, 0, s.Text.Length - 1);
                obj.AppendLine(ref s2, 0, 0, 0);
                base.Write(renderer, obj);
                
                obj.Lines.Clear();
                return;
            }

            
            base.Write(renderer, obj);

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