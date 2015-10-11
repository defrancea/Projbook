﻿using CommonMark;
using CommonMark.Syntax;
using EnsureThat;
using Projbook.Core.Exception;
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
        /// The csproj file of the documentation project.
        /// Snippets location will be determined from the file's director and project references.
        /// </summary>
        public FileInfo CsprojFile { get; private set; }

        /// <summary>
        /// The documentation template using razor syntax, see documentation for model structure.
        /// </summary>
        public FileInfo TemplateFile { get; private set; }

        /// <summary>
        /// The documentation template for pdf using razor syntax, see documentation for model structure.
        /// </summary>
        public FileInfo TemplateFilePdf { get; private set; }

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
        /// <param name="csprojFile">Initializes the required <see cref="CsprojFile"/>.</param>
        /// <param name="templateFilePath">Initializes the required <see cref="TemplateFile"/>.</param>
        /// <param name="configFile">Initializes the required <see cref="ConfigFile"/>.</param>
        /// <param name="outputDirectoryPath">Initializes the required <see cref="OutputDirectory"/>.</param>
        public ProjbookEngine(string csprojFile, string templateFilePath, string configFile, string outputDirectoryPath)
        {
            // Data validation
            Ensure.That(() => csprojFile).IsNotNullOrWhiteSpace();
            Ensure.That(() => templateFilePath).IsNotNullOrWhiteSpace();
            Ensure.That(() => configFile).IsNotNullOrWhiteSpace();
            Ensure.That(() => outputDirectoryPath).IsNotNullOrWhiteSpace();
            Ensure.That(File.Exists(csprojFile), string.Format("Could not find '{0}' file", csprojFile)).IsTrue();
            Ensure.That(File.Exists(templateFilePath), string.Format("Could not find '{0}' file", templateFilePath)).IsTrue();
            Ensure.That(File.Exists(configFile), string.Format("Could not find '{0}' file", configFile)).IsTrue();

            // Initialize
            this.CsprojFile = new FileInfo(csprojFile);
            this.TemplateFile = new FileInfo(templateFilePath);
            this.ConfigFile = new FileInfo(configFile);
            this.OutputDirectory = new DirectoryInfo(outputDirectoryPath);
            this.snippetExtractorFactory = new SnippetExtractorFactory(this.CsprojFile);

            // Compute and initialize pdf template
            string templateDirectory = Path.GetDirectoryName(templateFilePath);
            string pdfTemplateFile = string.Format("{0}-pdf{1}", Path.GetFileNameWithoutExtension(templateFilePath), Path.GetExtension(templateFilePath));
            this.TemplateFilePdf = new FileInfo(Path.Combine(templateDirectory, pdfTemplateFile));
        }

        /// <summary>
        /// Generates documentation.
        /// </summary>
        public Model.GenerationError[] Generate()
        {
            // Initialize the list containing all generation errors
            List<Model.GenerationError> generationError = new List<Model.GenerationError>();

            // Ensute output directory exists
            if (!this.OutputDirectory.Exists)
            {
                this.OutputDirectory.Create();
            }

            // Read configuration
            ConfigurationLoader configurationLoader = new ConfigurationLoader();
            Configuration configuration;
            try
            {
                configuration = configurationLoader.Load(this.ConfigFile);
            }
            catch (System.Exception exception)
            {
                generationError.Add(new Model.GenerationError(this.ConfigFile.FullName, string.Format("Error during loading configuration: {0}", exception.Message)));
                return generationError.ToArray();
            }
            
            // Process all pages
            List<Model.Page> pages = new List<Model.Page>();
            bool first = true;
            foreach (Page page in configuration.Pages)
            {
                // Declare formatter
                InjectAnchorHtmlFormatter formatter = null;

                // Load the document
                Block document;
                FileInfo fileInfo = new FileInfo(page.Path);

                // Skip the page if doesn't exist
                if (!fileInfo.Exists)
                {
                    generationError.Add(new Model.GenerationError(this.ConfigFile.FullName, string.Format("Error during loading configuration: Could not find file '{0}'", fileInfo.FullName)));
                    continue;
                }

                // Process the page
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
                            // Cleanup Projbook specific syntax
                            node.Block.FencedCodeData.Info = snippetExtractor.Language;

                            // Inject snippet
                            try
                            {
                                Model.Snippet snippet = snippetExtractor.Extract();
                                StringContent code = new StringContent();
                                code.Append(snippet.Content, 0, snippet.Content.Length);
                                node.Block.StringContent = code;
                            }
                            catch (SnippetExtractionException snippetExtraction)
                            {
                                generationError.Add(new Model.GenerationError(
                                    sourceFile: page.Path,
                                    message: string.Format("{0}: {1}", snippetExtraction.Message, snippetExtraction.Pattern)));
                            }
                            catch (System.Exception exception)
                            {
                                generationError.Add(new Model.GenerationError(
                                    sourceFile: page.Path,
                                    message: exception.Message));
                            }
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

            // Standard generation
            try
            {
                this.GenerateFile(this.TemplateFile.FullName, this.TemplateFile.FullName, configuration, pages);
            }
            catch (System.Exception exception)
            {
                generationError.Add(new Model.GenerationError(this.TemplateFile.FullName, string.Format("Error during HTML generation: {0}", exception.Message)));
            }

            // Pdf specific generation
            string pdfTemplate = this.TemplateFilePdf.Exists ? this.TemplateFilePdf.FullName : this.TemplateFile.FullName;
            try
            {
                this.GenerateFile(pdfTemplate, this.TemplateFilePdf.FullName, configuration, pages);
            }
            catch (System.Exception exception)
            {
                generationError.Add(new Model.GenerationError(pdfTemplate, string.Format("Error during PDF generation: {0}", exception.Message)));
            }

            // Return the generation errors
            return generationError.ToArray();
        }

        /// <summary>
        /// Generates a file.
        /// </summary>
        /// <param name="templateName">The template name use as input.</param>
        /// <param name="targetName">The target name used as output.</param>
        /// <param name="configuration">The configuration to inject.</param>
        /// <param name="pages">The pages to inject.</param>
        private void GenerateFile(string templateName, string targetName, Configuration configuration, List<Model.Page> pages)
        {
            // Generate final documentation from the template using razor engine
            string fileName = string.Format("{0}-generated{1}", Path.GetFileNameWithoutExtension(targetName), Path.GetExtension(targetName));
            string outputFileHtml = Path.Combine(this.OutputDirectory.FullName, fileName);
            using (var reader = new StreamReader(new FileStream(templateName, FileMode.Open)))
            using (var writer = new StreamWriter(new FileStream(outputFileHtml, FileMode.Create)))
            {
                var config = new TemplateServiceConfiguration();
                config.Language = Language.CSharp;
                config.EncodedStringFactory = new RawStringFactory();
                var service = RazorEngineService.Create(config);
                Engine.Razor = service;
                string processed = Engine.Razor.RunCompile(new LoadedTemplateSource(reader.ReadToEnd()), string.Empty, null, new { Title = configuration.Title, Pages = pages });
                writer.WriteLine(processed);
            }
        }
    }
}