using CommonMark;
using CommonMark.Syntax;
using EnsureThat;
using Projbook.Core.Markdown;
using Projbook.Core.Model.Configuration;
using Projbook.Core.Projbook.Core;
using Projbook.Core.Snippet;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;
using System.Collections.Generic;
using System.IO;

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
        /// Snippet extractor factory.
        /// </summary>
        private SnippetExtractorFactory snippetExtractorFactory;

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
            Ensure.That(Directory.Exists(sourceDirectoryPath), string.Format("Could not find '{0}' directory", sourceDirectoryPath)).IsTrue();
            Ensure.That(File.Exists(templateFilePath), string.Format("Could not find '{0}' file", templateFilePath)).IsTrue();
            Ensure.That(File.Exists(configFile), string.Format("Could not find '{0}' file", configFile)).IsTrue();

            // Initialize
            this.SourceDirectory = new DirectoryInfo(sourceDirectoryPath);
            this.TemplateFile = new FileInfo(templateFilePath);
            this.ConfigFile = new FileInfo(configFile);
            this.OutputDirectory = new DirectoryInfo(outputDirectoryPath);
            this.snippetExtractorFactory = new SnippetExtractorFactory(this.SourceDirectory);
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
            ConfigurationLoader configurationLoader = new ConfigurationLoader();
            Configuration configuration = configurationLoader.Load(this.ConfigFile);
            
            // Process all pages
            List<Model.Page> pages = new List<Model.Page>();
            bool first = true;
            foreach (Page page in configuration.Pages)
            {
                // Declare formatter
                InjectAnchorHtmlFormatter formatter = null;

                // Load the document
                Block document;
                using (StreamReader reader = new StreamReader(new FileStream(new FileInfo(page.Path).FullName, FileMode.Open)))
                {
                    document = CommonMarkConverter.ProcessStage1(reader);
                }

                // Process snippet
                CommonMarkConverter.ProcessStage2(document);
                foreach (var node in document.AsEnumerable())
                {
                    // Filter fenced code
                    if (node.Block != null && node.Block.Tag == BlockTag.FencedCode)
                    {
                        // Extract snippet
                        string fencedCode = node.Block.FencedCodeData.Info;
                        ISnippetExtractor snippetExtractor = this.snippetExtractorFactory.CreateExtractor(fencedCode);

                        // Extract and inject snippet and the factory were able to create an extractor
                        if (null != snippetExtractor)
                        {
                            Model.Snippet snippet = snippetExtractor.Extract();
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
                        formatter = new InjectAnchorHtmlFormatter(page.Path, output, settings);
                        formatter.WriteDocument(d);
                    };

                // Write to output
                MemoryStream documentStream = new MemoryStream();
                using (StreamWriter writer = new StreamWriter(documentStream))
                {
                    CommonMarkConverter.ProcessStage3(document, writer);
                }

                // Add new page
                pages.Add(new Model.Page(
                    id: page.Path.Replace(".", string.Empty).Replace("/", string.Empty),
                    title: page.Title,
                    isHome: first,
                    anchor: formatter.Anchors,
                    content: System.Text.Encoding.UTF8.GetString(documentStream.ToArray())));
                first = false;
            }

            // Generate final documentation from the template using razor engine
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
                processed = Engine.Razor.RunCompile(new LoadedTemplateSource(reader.ReadToEnd()), string.Empty, null, new { Title = configuration.Title, Pages = pages });
                writer.WriteLine(processed);
            }
        }
    }
}