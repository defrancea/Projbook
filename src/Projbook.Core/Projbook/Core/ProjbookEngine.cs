using CommonMark;
using CommonMark.Syntax;
using EnsureThat;
using Projbook.Core.Exception;
using Projbook.Core.Markdown;
using Projbook.Core.Model.Configuration;
using Projbook.Core.Snippet;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using WkHtmlToXSharp;

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
        /// The configuration.
        /// </summary>
        public Configuration Configuration { get; private set; }

        /// <summary>
        /// The output directory where generated content will be written.
        /// </summary>
        public DirectoryInfo OutputDirectory { get; private set; }

        /// <summary>
        /// Snippet extractor factory.
        /// </summary>
        private SnippetExtractorFactory snippetExtractorFactory;

        /// <summary>
        /// Extractor cache that is bound to a snippet file name.
        /// </summary>
        private Dictionary<string, ISnippetExtractor> extractorCache = new Dictionary<string, ISnippetExtractor>();

        /// <summary>
        /// HResult representing an incorrect format during native dll loading.
        /// </summary>
        private const int INCORRECT_FORMAT_HRESULT = unchecked((int)0x8007000B);

        /// <summary>
        /// Initializes a new instance of <see cref="ProjbookEngine"/>.
        /// </summary>
        /// <param name="csprojFile">Initializes the required <see cref="CsprojFile"/>.</param>
        /// <param name="configuration">Initializes the required <see cref="Configuration"/>.</param>
        /// <param name="outputDirectoryPath">Initializes the required <see cref="OutputDirectory"/>.</param>
        public ProjbookEngine(string csprojFile, Configuration configuration, string outputDirectoryPath)
        {
            // Data validation
            Ensure.That(() => csprojFile).IsNotNullOrWhiteSpace();
            Ensure.That(() => configuration).IsNotNull();
            Ensure.That(() => outputDirectoryPath).IsNotNullOrWhiteSpace();
            Ensure.That(File.Exists(csprojFile), string.Format("Could not find '{0}' file", csprojFile)).IsTrue();

            // Initialize
            this.CsprojFile = new FileInfo(csprojFile);
            this.Configuration = configuration;
            this.OutputDirectory = new DirectoryInfo(outputDirectoryPath);
            this.snippetExtractorFactory = new SnippetExtractorFactory(this.CsprojFile);
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
            
            // Process all pages
            List<Model.Page> pages = new List<Model.Page>();
            foreach (Page page in this.Configuration.Pages)
            {
                // Compute the page id used as a tab id and page prefix for bookmarking
                string pageId = page.Path.Replace(".", string.Empty).Replace("/", string.Empty);

                // Load the document
                Block document;

                // Process the page
                using (StreamReader reader = new StreamReader(new FileStream(new FileInfo(page.FileSystemPath).FullName, FileMode.Open, FileAccess.Read)))
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
                        // Buil extraction rule
                        string fencedCode = node.Block.FencedCodeData.Info;
                        SnippetExtractionRule snippetExtractionRule = SnippetExtractionRule.Parse(fencedCode);
                        
                        // Extract and inject snippet and the factory were able to create an extractor
                        if (null != snippetExtractionRule)
                        {
                            // Cleanup Projbook specific syntax
                            node.Block.FencedCodeData.Info = snippetExtractionRule.Language;

                            // Inject snippet
                            try
                            {
                                // Retrieve the extractor instance
                                ISnippetExtractor snippetExtractor;
                                if (!this.extractorCache.TryGetValue(snippetExtractionRule.FileName, out snippetExtractor))
                                {
                                    snippetExtractor = this.snippetExtractorFactory.CreateExtractor(snippetExtractionRule);
                                    this.extractorCache[snippetExtractionRule.FileName] = snippetExtractor;
                                }

                                // Look for the file in available source directories
                                FileInfo fileInfo = null;
                                DirectoryInfo[] directoryInfos = this.ExtractSourceDirectories(this.CsprojFile);
                                foreach (DirectoryInfo directoryInfo in directoryInfos)
                                {
                                    string fullFilePath = Path.Combine(directoryInfo.FullName, snippetExtractionRule.FileName ?? string.Empty);
                                    if (File.Exists(fullFilePath))
                                    {
                                        fileInfo = new FileInfo(fullFilePath);
                                        break;
                                    }
                                }

                                // Raise an error if cannot find the file
                                if (null == fileInfo)
                                {
                                    // Locate block line
                                    int line = this.LocateBlockLine(node.Block, page);

                                    // Compute error column: Index of the path in the fenced code + 3 (for the ``` prefix) + 1 (to be 1 based)
                                    int column = fencedCode.IndexOf(snippetExtractionRule.FileName) + 4;

                                    // Report error
                                    generationError.Add(new Model.GenerationError(
                                        sourceFile: page.Path,
                                        message: string.Format("Cannot find file '{0}' in any referenced project ({0})", snippetExtractionRule.FileName, string.Join(";", directoryInfos.Select(x => x.FullName))),
                                        line: line,
                                        column: column));
                                    continue;
                                }

                                // Extract the snippet
                                Model.Snippet snippet = null;
                                using (StreamReader streamReader = new StreamReader(fileInfo.OpenRead()))
                                {
                                    snippet = snippetExtractor.Extract(streamReader, snippetExtractionRule.Pattern);
                                }
                                StringContent code = new StringContent();
                                code.Append(snippet.Content, 0, snippet.Content.Length);
                                node.Block.StringContent = code;
                            }
                            catch (SnippetExtractionException snippetExtraction)
                            {
                                // Locate block line
                                int line = this.LocateBlockLine(node.Block, page);

                                // Compute error column: Fenced code length - pattern length + 3 (for the ``` prefix) + 1 (to be 1 based)
                                int column = fencedCode.Length - snippetExtractionRule.Pattern.Length + 4;

                                // Report error
                                generationError.Add(new Model.GenerationError(
                                    sourceFile: page.Path,
                                    message: string.Format("{0}: {1}", snippetExtraction.Message, snippetExtraction.Pattern),
                                    line: line,
                                    column: column));
                            }
                            catch (System.Exception exception)
                            {
                                generationError.Add(new Model.GenerationError(
                                    sourceFile: page.Path,
                                    message: exception.Message,
                                    line: 0,
                                    column: 0));
                            }
                        }
                    }
                }

                // Write to output
                ProjbookHtmlFormatter projbookHtmlFormatter = null;
                MemoryStream documentStream = new MemoryStream();
                using (StreamWriter writer = new StreamWriter(documentStream))
                {
                    // Setup custom formatter
                    CommonMarkSettings.Default.OutputDelegate = (d, o, s) => (projbookHtmlFormatter = new ProjbookHtmlFormatter(pageId, o, s, this.Configuration.SectionTitleBase)).WriteDocument(d);

                    // Render
                    CommonMarkConverter.ProcessStage3(document, writer);
                }

                // Initialize the pre section content
                string preSectionContent = string.Empty;
                
                // Retrieve page content
                byte[] pageContent = documentStream.ToArray();

                // Set the whole page content if no page break is detected
                if (projbookHtmlFormatter.PageBreak.Length == 0)
                {
                    preSectionContent = System.Text.Encoding.UTF8.GetString(pageContent);
                }

                // Compute pre section content from the position 0 to the first page break position
                if (projbookHtmlFormatter.PageBreak.Length > 0 && projbookHtmlFormatter.PageBreak.First().Position > 0)
                {
                    preSectionContent = this.StringFromByteArray(pageContent, 0, projbookHtmlFormatter.PageBreak.First().Position);
                }

                // Build section list
                List<Model.Section> sections = new List<Model.Section>();
                for (int i = 0; i < projbookHtmlFormatter.PageBreak.Length; ++i)
                {
                    // Retrieve the current page break
                    PageBreakInfo pageBreak = projbookHtmlFormatter.PageBreak[i];

                    // Extract the content from the current page break to the next one if any
                    string content = null;
                    if (i < projbookHtmlFormatter.PageBreak.Length - 1)
                    {
                        PageBreakInfo nextBreak = projbookHtmlFormatter.PageBreak[1 + i];
                        content = this.StringFromByteArray(pageContent, pageBreak.Position, nextBreak.Position - pageBreak.Position);
                    }

                    // Otherwise extract the content from the current page break to the end of the content
                    else
                    {
                        content = this.StringFromByteArray(pageContent, pageBreak.Position, pageContent.Length - pageBreak.Position);
                    }
                    
                    // Create a new section and add to the known list
                    sections.Add(new Model.Section(
                        id: pageBreak.Id,
                        title: pageBreak.Title,
                        content: content));
                }

                // Add new page
                pages.Add(new Model.Page(
                    id: pageId,
                    title: page.Title,
                    preSectionContent: preSectionContent,
                    sections: sections.ToArray()));
            }

            // Html generation
            if (this.Configuration.GenerateHtml)
            {
                try
                {
                    string outputFileHtml = Path.Combine(this.OutputDirectory.FullName, this.Configuration.OutputHtml);
                    this.GenerateFile(this.Configuration.TemplateHtml, outputFileHtml, this.Configuration, pages);  
                }
                catch (TemplateParsingException templateParsingException)
                {
                    generationError.Add(new Model.GenerationError(this.Configuration.TemplateHtml, string.Format("Error during HTML generation: {0}", templateParsingException.Message), templateParsingException.Line, templateParsingException.Column));
                }
                catch (System.Exception exception)
                {
                    generationError.Add(new Model.GenerationError(this.Configuration.TemplateHtml, string.Format("Error during HTML generation: {0}", exception.Message), 0, 0));
                }
            }

            // Pdf generation
            if (this.Configuration.GeneratePdf)
            {
                try
                {
                    // Generate the pdf template
                    string outputFileHtml = Path.Combine(this.OutputDirectory.FullName, this.Configuration.OutputPdf);
                    this.GenerateFile(this.Configuration.TemplatePdf, outputFileHtml, this.Configuration, pages);

#if !NOPDF
                    // Register bundles
                    WkHtmlToXLibrariesManager.Register(new Linux32NativeBundle());
                    WkHtmlToXLibrariesManager.Register(new Linux64NativeBundle());
                    WkHtmlToXLibrariesManager.Register(new Win32NativeBundle());
                    WkHtmlToXLibrariesManager.Register(new Win64NativeBundle());

                    // Compute file names
                    string outputPdf = Path.ChangeExtension(this.Configuration.OutputPdf, ".pdf");
                    string outputFilePdf = Path.Combine(this.OutputDirectory.FullName, outputPdf);

                    // Prepare the converter
                    MultiplexingConverter pdfConverter = new MultiplexingConverter();
                    pdfConverter.ObjectSettings.Page = outputFileHtml;
                    pdfConverter.Error += (s, e) => {
                        generationError.Add(new Model.GenerationError(this.Configuration.TemplatePdf, string.Format("Error during PDF generation: {0}", e.Value), 0, 0));
                    };

                    // Run pdf convertion
                    using (pdfConverter)
                    using (var outputFileStream = new FileStream(outputFilePdf, FileMode.Create, FileAccess.Write))
                    {
                        try
                        {
                            byte[] buffer = pdfConverter.Convert();
                            outputFileStream.Write(buffer, 0, buffer.Length);
                        }
                        catch
                        {
                            // Ignore generation errors at that level
                            // Errors are handled by the error handling having the best description
                        }
                    }
#endif 
                }
                catch (TemplateParsingException templateParsingException)
                {
                    generationError.Add(new Model.GenerationError(this.Configuration.TemplatePdf, string.Format("Error during PDF generation: {0}", templateParsingException.Message), templateParsingException.Line, templateParsingException.Column));
                }
                catch (System.Exception exception)
                {
                    if (null != exception.InnerException && INCORRECT_FORMAT_HRESULT == exception.InnerException.HResult)
                    {
                        // Report detailed error message for wrong architecture loading
                        string runningArchitectureProccess = IntPtr.Size == 8 ? "x64" : "x86";
                        string otherRunningArchitectureProccess = IntPtr.Size != 8 ? "x64" : "x86";
                        generationError.Add(new Model.GenerationError(this.Configuration.TemplatePdf, string.Format("Error during PDF generation: Could not load wkhtmltopdf for {0}. Try again running as a {1} process.", runningArchitectureProccess, otherRunningArchitectureProccess), 0, 0));
                    }
                    else
                    {
                        // Report unknown error
                        generationError.Add(new Model.GenerationError(this.Configuration.TemplatePdf, string.Format("Error during PDF generation: {0}", exception.Message), 0, 0));
                    }
                }
            }

            // Return the generation errors
            return generationError.ToArray();
        }

        /// <summary>
        /// Extracts a string from a byte array.
        /// </summary>
        /// <param name="data">The data from where extracting the string.</param>
        /// <param name="startIndex">The start index from where building the string.</param>
        /// <param name="length">The length of byte to be used as data source.</param>
        /// <returns>The built string.</returns>
        private string StringFromByteArray(byte[] data, long startIndex, long length)
        {
            byte[] subData = new byte[length];
            Array.Copy(data, startIndex, subData, 0, length);
            return System.Text.Encoding.UTF8.GetString(subData);
        }

        /// <summary>
        /// Generates a file.
        /// </summary>
        /// <param name="templateName">The template name use as input.</param>
        /// <param name="outputFileHtml">The output html file.</param>
        /// <param name="configuration">The configuration to inject.</param>
        /// <param name="pages">The pages to inject.</param>
        private void GenerateFile(string templateName, string outputFileHtml, Configuration configuration, List<Model.Page> pages)
        {
            // Generate final documentation from the template using razor engine
            using (var reader = new StreamReader(new FileStream(templateName, FileMode.Open, FileAccess.Read)))
            using (var writer = new StreamWriter(new FileStream(outputFileHtml, FileMode.Create, FileAccess.Write)))
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
        
        /// <summary>
        /// Computes line number of a block in a page.
        /// </summary>
        /// <param name="block">The block to locate.</param>
        /// <param name="page">The page where the block is.</param>
        /// <returns>The block line.</returns>
        private int LocateBlockLine(Block block, Page page)
        {
            // Initialize the line to 1
            int line = 1;

            // Load the page until the block source position
            char[] buffer = new char[block.SourcePosition];
            using (StreamReader reader = new StreamReader(new FileStream(new FileInfo(page.Path).FullName, FileMode.Open, FileAccess.Read)))
            {
                reader.Read(buffer, 0, buffer.Length);
            }

            // Count the number of line break to increment line number
            foreach (char currentChar in buffer)
            {
                if ('\n' == currentChar)
                {
                    ++line;
                }
            }

            // Return the line number
            return line;
        }

        /// <summary>
        /// Extracts source directories from the csproj file.
        /// The extected directory info list is the csproj's directory and all project references.
        /// </summary>
        /// <param name="csprojFile"></param>
        /// <returns></returns>
        private DirectoryInfo[] ExtractSourceDirectories(FileInfo csprojFile)
        {
            // Data validation
            Ensure.That(() => csprojFile).IsNotNull();
            Ensure.That(csprojFile.Exists, string.Format("Could not find '{0}' file", csprojFile)).IsTrue();

            // Initialize the extracted directories
            List<DirectoryInfo> extractedSourceDirectories = new List<DirectoryInfo>();

            // Add the csproj's directory
            DirectoryInfo projectDirectory = new DirectoryInfo(Path.GetDirectoryName(csprojFile.FullName));
            extractedSourceDirectories.Add(projectDirectory);

            // Extract project reference path
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(csprojFile.FullName);
            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
            xmlNamespaceManager.AddNamespace("msbuild", "http://schemas.microsoft.com/developer/msbuild/2003");
            XmlNodeList xmlNodes = xmlDocument.SelectNodes("//msbuild:ProjectReference", xmlNamespaceManager);
            for (int i = 0; i < xmlNodes.Count; ++i)
            {
                XmlNode xmlNode = xmlNodes.Item(i);
                string includeValue = xmlNode.Attributes["Include"].Value;
                string combinedPath = Path.Combine(projectDirectory.FullName, includeValue);

                // The combinedPath can contains both forward and backslash path chunk.
                // In linux environment we can end up having "/..\" in the path which make the GetDirectoryName method bugging (returns empty).
                // For this reason we need to make sure that the combined path uses forward slashes
                combinedPath = combinedPath.Replace(@"\", "/");

                // Add the combined path
                extractedSourceDirectories.Add(new DirectoryInfo(Path.GetDirectoryName(Path.GetFullPath(combinedPath))));
            }

            // Returne the extracted directories
            return extractedSourceDirectories.ToArray();
        }
    }
}