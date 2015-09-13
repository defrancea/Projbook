using CommonMark;
using CommonMark.Syntax;
using EnsureThat;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using Projbook.Core.Markdown;
using Projbook.Core.Model;
using Projbook.Core.Snippet;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Projbook.Core
{
    /// <summary>
    /// Entry point for document generation.
    /// Setup your generation environment using the right constructor and run the process with <seealso cref="Generate()"/> method.
    /// </summary>
    public class ProjbookEngine
    {
        /// <summary>
        /// Source directory where source code is.
        /// Snippets are will be looked up from this location.
        /// </summary>
        public DirectoryInfo SourceDirectory { get; private set; }
        
        /// <summary>
        /// The documentation template using razon syntax, see documentation for model structure.
        /// </summary>
        public FileInfo TemplateFile { get; private set; }

        /// <summary>
        /// The config file defining documentation pages and labels.
        /// </summary>
        public FileInfo ConfigFile { get; private set; }

        /// <summary>
        /// The output directory where generated content will be written.
        /// </summary>
        public DirectoryInfo OutputDirectory { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="ProjbookEngine"/>.
        /// </summary>
        /// <param name="sourceDirectoryPath">Initializes the required <see cref="SourceDirectory"/>.</param>
        /// <param name="templateFilePath">Initializes the required <see cref="TemplateFile"/>.</param>
        /// <param name="configFile">Initializes the required <see cref="ConfigFile"/>.</param>
        /// <param name="outputDirectoryPath">Initializes the required <see cref="OutputDirectory"/>.</param>
        public ProjbookEngine(string sourceDirectoryPath, string templateFilePath, string configFile, string outputDirectoryPath)
        {
            // Data validation
            Ensure.That(() => sourceDirectoryPath).IsNotNullOrWhiteSpace();
            Ensure.That(() => templateFilePath).IsNotNullOrWhiteSpace();
            Ensure.That(() => configFile).IsNotNullOrWhiteSpace();
            Ensure.That(() => outputDirectoryPath).IsNotNullOrWhiteSpace();
            Ensure.That(Directory.Exists(sourceDirectoryPath)).IsTrue();
            Ensure.That(File.Exists(templateFilePath)).IsTrue();
            Ensure.That(File.Exists(configFile)).IsTrue();

            this.SourceDirectory = new DirectoryInfo(sourceDirectoryPath);
            this.TemplateFile = new FileInfo(templateFilePath);
            this.ConfigFile = new FileInfo(configFile);
            this.OutputDirectory = new DirectoryInfo(outputDirectoryPath);
        }

        /// <summary>
        /// Generates documentation.
        /// </summary>
        public void Generate()
        {
            // Ensute output directory exists
            if (!this.OutputDirectory.Exists)
            {
                this.OutputDirectory.Create();
            }

            // Read configuration
            // Todo: Remove dynamic and use an actual model for configuration
            Dictionary<string, object> configuration = null;
            using (var reader = new StreamReader(new FileStream(this.ConfigFile.FullName, FileMode.Open)))
            {
                configuration = JsonConvert.DeserializeObject<Dictionary<string, object>>(reader.ReadToEnd());
            }
            List<DocumentationPage> documentationPages = new List<DocumentationPage>();
            string documentationTitle;
            foreach (string key in configuration.Keys)
            {
                // Isolate page entry
                if (key.StartsWith("page."))
                {
                    dynamic page = configuration[key];
                    string pageId = key.Substring("page.".Length);
                    FileInfo pageFile = new FileInfo(pageId);
                    string pageTitle = page.title;
                    int pageIndex = page.index;
                    pageId = pageId.Replace(".", string.Empty);

                    // Create documentation page
                    documentationPages.Add(new DocumentationPage(pageId, pageTitle, pageFile, pageIndex));
                }
            }
            documentationPages = documentationPages.OrderBy(x => x.Index).ToList(); // Todo: Avoid useless intermediaite data structure
            documentationPages[0].IsHome = true;
            documentationTitle = (string)configuration["main.title"];
            
            // Initializes snippet extractor
            SnippetExtractor snippetExtractor = new SnippetExtractor(this.SourceDirectory);

            //SortedDictionary<string, object> pages = new SortedDictionary<string, object>();
            foreach (DocumentationPage documentationPage in documentationPages)
            {
                // Declare formatter
                InjectAnchorHtmlFormatter formatter = null;

                // Load the documentation
                // Todo: reduce using scopes
                MemoryStream documentStream = new MemoryStream();
                using (var reader = new StreamReader(new FileStream(documentationPage.File.FullName, FileMode.Open)))
                using (var writer = new StreamWriter(documentStream))
                {
                    // Process two first stages in order to retrieve a model to work on
                    Block document = CommonMarkConverter.ProcessStage1(reader);
                    CommonMarkConverter.ProcessStage2(document);
                    
                    // Process snippet
                    Regex regex = new Regex(@"^(.+)\[(.+)\]$", RegexOptions.Compiled);
                    foreach (var node in document.AsEnumerable())
                    {
                        // Filter fenced code
                        if (node.Block != null && node.Block.Tag == BlockTag.FencedCode)
                        {
                            // Extract snippet pattern
                            string fencedCode = node.Block.FencedCodeData.Info;
                            Match match = regex.Match(fencedCode);
                            if (match.Success)
                            {
                                // Retrieve code language and rule pattern
                                string language = match.Groups[1].Value;
                                string rulePattern = match.Groups[2].Value;

                                // Extract the snippet
                                Model.Snippet snippet = snippetExtractor.Extract(rulePattern);
                                StringContent code = new StringContent();
                                code.Append(snippet.Content, 0, snippet.Content.Length);
                                node.Block.StringContent = code;
                            }
                        }
                    }

                    // Setup custom formatter
                    CommonMarkSettings.Default.OutputDelegate =
                        (d, output, settings) =>
                        {
                            formatter = new InjectAnchorHtmlFormatter(documentationPage.Id, output, settings);
                            formatter.WriteDocument(d);
                        };

                    // Render the documentation page
                    CommonMarkConverter.ProcessStage3(document, writer);
                }

                // Set anchors found during the rendering
                documentationPage.Anchors = formatter.Anchors;
                documentationPage.Content = System.Text.Encoding.UTF8.GetString(documentStream.ToArray());
            }

            // Generate final documentation from the template using razor engine
            // Todo: Split loading and razor models
            string processed;
            string outputFile = Path.Combine(this.OutputDirectory.FullName, "generated.html");
            using (var reader = new StreamReader(new FileStream(this.TemplateFile.FullName, FileMode.Open)))
            using (var writer = new StreamWriter(new FileStream(outputFile, FileMode.Create)))
            {
                var config = new TemplateServiceConfiguration();
                config.Language = Language.CSharp;
                config.EncodedStringFactory = new RawStringFactory();
                var service = RazorEngineService.Create(config);
                Engine.Razor = service;
                processed = Engine.Razor.RunCompile(new LoadedTemplateSource(reader.ReadToEnd()), string.Empty, null, new { Title = documentationTitle, Pages = documentationPages });
                writer.WriteLine(processed);
            }
        }
    }
}