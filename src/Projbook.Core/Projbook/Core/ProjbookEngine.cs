using CommonMark;
using CommonMark.Syntax;
using EnsureThat;
using Projbook.Core.Markdown;
using Projbook.Core.Model.Configuration;
using Projbook.Core.Snippet;
using Projbook.Extension.Exception;
using Projbook.Extension.Spi;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Xml;
using WkHtmlToXSharp;
using Projbook.Core.Model;
using Page = Projbook.Core.Model.Configuration.Page;

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
        public FileInfoBase CsprojFile { get; private set; }

        /// <summary>
        /// The configurations.
        /// </summary>
        public Configuration[] Configurations { get; private set; }

        /// <summary>
        /// The output directory where generated content will be written.
        /// </summary>
        public DirectoryInfoBase OutputDirectory { get; private set; }

        /// <summary>
        /// The file system abstraction.
        /// </summary>
        private readonly IFileSystem fileSystem;

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
        /// <param name="fileSystem">Initializes the required file system abstraction.</param>
        /// <param name="csprojFile">Initializes the required <see cref="CsprojFile"/>.</param>
        /// <param name="configuration">Initializes the required <see cref="Configuration"/>.</param>
        /// <param name="outputDirectoryPath">Initializes the required <see cref="OutputDirectory"/>.</param>
        public ProjbookEngine(IFileSystem fileSystem, string csprojFile, Configuration[] configurations, string outputDirectoryPath)
        {
            // Data validation
            Ensure.That(() => fileSystem).IsNotNull();
            Ensure.That(() => csprojFile).IsNotNullOrWhiteSpace();
            Ensure.That(() => configurations).HasItems();
            Ensure.That(() => outputDirectoryPath).IsNotNullOrWhiteSpace();
            Ensure.That(fileSystem.File.Exists(csprojFile), string.Format("Could not find '{0}' file", csprojFile)).IsTrue();

            // Initialize
            this.fileSystem = fileSystem;
            this.CsprojFile = this.fileSystem.FileInfo.FromFileName(csprojFile);
            this.Configurations = configurations;
            this.OutputDirectory = this.fileSystem.DirectoryInfo.FromDirectoryName(outputDirectoryPath);
            this.snippetExtractorFactory = new SnippetExtractorFactory();
        }

        public GenerationError[] GenerateAll()
        {
            // Run generation for each configuration
            List<GenerationError> errors = new List<GenerationError>();
            foreach (Configuration configuration in this.Configurations)
            {
                // Generate the documentation
                errors.AddRange(this.Generate(configuration));
            }

            // Generate the index html file for the configurations that call for html generation
            Configuration[] htmlConfigurations = this.Configurations.Where(configuration => configuration.GenerateHtml).ToArray();
            if (htmlConfigurations.Length > 0)
            {
                this.GenerateIndex(htmlConfigurations);
            }

            // Report processing successful
            return errors.ToArray();
        }

        /// <summary>
        /// Generates documentation.
        /// </summary>
        public Model.GenerationError[] Generate(Configuration configuration)
        {
            // Initialize the list containing all generation errors
            List<Model.GenerationError> generationError = new List<Model.GenerationError>();

            // Ensure output directory exists
            if (!this.OutputDirectory.Exists)
            {
                this.OutputDirectory.Create();
            }
            
            // Process all pages
            List<Model.Page> pages = new List<Model.Page>();
            foreach (Page page in configuration.Pages)
            {
                // Compute the page id used as a tab id and page prefix for bookmarking
                string pageId = page.Path.Replace(".", string.Empty).Replace("/", string.Empty);

                // Load the document
                Block document;

                // Process the page
                string pageFilePath = this.fileSystem.FileInfo.FromFileName(page.FileSystemPath).FullName;
                using (StreamReader reader = new StreamReader(this.fileSystem.File.Open(pageFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
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
                        // Build extraction rule
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
                                if (!this.extractorCache.TryGetValue(snippetExtractionRule.TargetPath, out snippetExtractor))
                                {
                                    snippetExtractor = this.snippetExtractorFactory.CreateExtractor(snippetExtractionRule);
                                    this.extractorCache[snippetExtractionRule.TargetPath] = snippetExtractor;
                                }

                                // Look for the file in available source directories
                                FileSystemInfoBase fileSystemInfo = null;
                                DirectoryInfoBase[] directoryInfos = null;
                                if (TargetType.FreeText != snippetExtractor.TargetType)
                                {
                                    directoryInfos = this.ExtractSourceDirectories(this.CsprojFile);
                                    foreach (DirectoryInfoBase directoryInfo in directoryInfos)
                                    {
                                        // Get directory name
                                        string directoryName = directoryInfo.FullName;
                                        if (1 == directoryName.Length && Path.DirectorySeparatorChar == directoryName[0])
                                            directoryName = string.Empty;

                                        // Compute file full path
                                        string fullFilePath = this.fileSystem.Path.Combine(directoryName, snippetExtractionRule.TargetPath ?? string.Empty);
                                        switch (snippetExtractor.TargetType)
                                        {
                                            case TargetType.File:
                                                if (this.fileSystem.File.Exists(fullFilePath))
                                                {
                                                    fileSystemInfo = this.fileSystem.FileInfo.FromFileName(fullFilePath);
                                                }
                                                break;
                                            case TargetType.Folder:
                                                if (this.fileSystem.Directory.Exists(fullFilePath))
                                                {
                                                    fileSystemInfo = this.fileSystem.DirectoryInfo.FromDirectoryName(fullFilePath);
                                                }
                                                break;
                                        }

                                        // Stop lookup if the file system info is found
                                        if (null != fileSystemInfo)
                                        {
                                            break;
                                        }
                                    }
                                }

                                // Raise an error if cannot find the file
                                if (null == fileSystemInfo && TargetType.FreeText != snippetExtractor.TargetType)
                                {
                                    // Locate block line
                                    int line = this.LocateBlockLine(node.Block, page);

                                    // Compute error column: Index of the path in the fenced code + 3 (for the ``` prefix) + 1 (to be 1 based)
                                    int column = fencedCode.IndexOf(snippetExtractionRule.TargetPath) + 4;

                                    // Report error
                                    generationError.Add(new Model.GenerationError(
                                        sourceFile: page.Path,
                                        message: string.Format("Cannot find target '{0}' in any referenced project ({0})", snippetExtractionRule.TargetPath, string.Join(";", directoryInfos.Select(x => x.FullName))),
                                        line: line,
                                        column: column));
                                    continue;
                                }

                                // Extract the snippet
                                Extension.Model.Snippet snippet =
                                    TargetType.FreeText == snippetExtractor.TargetType
                                    ? snippetExtractor.Extract(null, snippetExtractionRule.TargetPath)
                                    : snippetExtractor.Extract(fileSystemInfo, snippetExtractionRule.Pattern);

                                // Inject snippet
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
                    CommonMarkSettings.Default.OutputDelegate = (d, o, s) => (projbookHtmlFormatter = new ProjbookHtmlFormatter(pageId, o, s, configuration.SectionTitleBase)).WriteDocument(d);

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
            if (configuration.GenerateHtml)
            {
                try
                {
                    string outputFileHtml = this.fileSystem.Path.Combine(this.OutputDirectory.FullName, configuration.OutputHtml);
                    this.GenerateFile(configuration.TemplateHtml, outputFileHtml, configuration, pages);  
                }
                catch (TemplateParsingException templateParsingException)
                {
                    generationError.Add(new Model.GenerationError(configuration.TemplateHtml, string.Format("Error during HTML generation: {0}", templateParsingException.Message), templateParsingException.Line, templateParsingException.Column));
                }
                catch (System.Exception exception)
                {
                    generationError.Add(new Model.GenerationError(configuration.TemplateHtml, string.Format("Error during HTML generation: {0}", exception.Message), 0, 0));
                }
            }

            // Pdf generation
            if (configuration.GeneratePdf)
            {
                try
                {
                    // Generate the pdf template
                    string outputFileHtml = this.fileSystem.Path.Combine(this.OutputDirectory.FullName, configuration.OutputPdf);
                    this.GenerateFile(configuration.TemplatePdf, outputFileHtml, configuration, pages);

#if !NOPDF
                    // Register bundles
                    WkHtmlToXLibrariesManager.Register(new Linux32NativeBundle());
                    WkHtmlToXLibrariesManager.Register(new Linux64NativeBundle());
                    WkHtmlToXLibrariesManager.Register(new Win32NativeBundle());
                    WkHtmlToXLibrariesManager.Register(new Win64NativeBundle());

                    // Compute file names
                    string outputPdf = this.fileSystem.Path.ChangeExtension(configuration.OutputPdf, ".pdf");
                    string outputFilePdf = this.fileSystem.Path.Combine(this.OutputDirectory.FullName, outputPdf);

                    // Prepare the converter
                    MultiplexingConverter pdfConverter = new MultiplexingConverter();
                    pdfConverter.ObjectSettings.Page = outputFileHtml;
                    pdfConverter.Error += (s, e) => {
                        generationError.Add(new Model.GenerationError(configuration.TemplatePdf, string.Format("Error during PDF generation: {0}", e.Value), 0, 0));
                    };

                    // Prepare file system if abstracted
                    bool requireCopyToFileSystem = !File.Exists(outputFileHtml);
                    try
                    {
                        // File system may be abstracted, this requires to copy the pdf generation file to the actual file system
                        // in order to allow wkhtmltopdf to process the generated html as input file
                        if (requireCopyToFileSystem)
                        {
                            File.WriteAllBytes(outputFileHtml, this.fileSystem.File.ReadAllBytes(outputFileHtml));
                        }
                        
                        // Run pdf converter
                        using (pdfConverter)
                        using (Stream outputFileStream = this.fileSystem.File.Open(outputFilePdf, FileMode.Create, FileAccess.Write, FileShare.None))
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
                    }
                    finally
                    {
                        if (requireCopyToFileSystem && File.Exists(outputFileHtml))
                        {
                            File.Delete(outputFileHtml);
                        }
                    }
#endif 
                }
                catch (TemplateParsingException templateParsingException)
                {
                    generationError.Add(new Model.GenerationError(configuration.TemplatePdf, string.Format("Error during PDF generation: {0}", templateParsingException.Message), templateParsingException.Line, templateParsingException.Column));
                }
                catch (System.Exception exception)
                {
                    if (null != exception.InnerException && INCORRECT_FORMAT_HRESULT == exception.InnerException.HResult)
                    {
                        // Report detailed error message for wrong architecture loading
                        string runningArchitectureProccess = IntPtr.Size == 8 ? "x64" : "x86";
                        string otherRunningArchitectureProccess = IntPtr.Size != 8 ? "x64" : "x86";
                        generationError.Add(new Model.GenerationError(configuration.TemplatePdf, string.Format("Error during PDF generation: Could not load wkhtmltopdf for {0}. Try again running as a {1} process.", runningArchitectureProccess, otherRunningArchitectureProccess), 0, 0));
                    }
                    else
                    {
                        // Report unknown error
                        generationError.Add(new Model.GenerationError(configuration.TemplatePdf, string.Format("Error during PDF generation: {0}", exception.Message), 0, 0));
                    }
                }
            }

            // Return the generation errors
            return generationError.ToArray();
        }

        /// <summary>
        /// Generates the index page for the documentation.
        /// </summary>
        public void GenerateIndex(Configuration[] configurations)
        {
            // Try to generate the index html file
            try
            {
                string outputFileHtml = this.fileSystem.Path.Combine(this.OutputDirectory.FullName, "index.html");
                this.GenerateIndexFile("index-template.html", outputFileHtml, configurations);
            }
            catch
            {
                // For now suppress errors from generating the index file
            }
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
            var fileConfiguration = new { Title = configuration.Title, Pages = pages };
            this.WriteFile(templateName, outputFileHtml, fileConfiguration);
        }

        /// <summary>
        /// Generates the index file.
        /// </summary>
        /// <param name="templateName">The template name.</param>
        /// <param name="outputFileHtml">The file to output to.</param>
        /// <param name="configurations">The configuration.</param>
        private void GenerateIndexFile(string templateName, string outputFileHtml, Configuration[] configurations)
        {
            // Generate the index html for the configurations using Razor
            var fileConfiguration = new { Configurations = configurations };
            this.WriteFile(templateName, outputFileHtml, fileConfiguration);
        }

        /// <summary>
        /// Write to the documentation file.
        /// </summary>
        /// <param name="templateName">The template name.</param>
        /// <param name="outputFileHtml">The file to output to.</param>
        /// <param name="fileConfiguration">The file configuration object.</param>
        private void WriteFile(string templateName, string outputFileHtml, Object fileConfiguration)
        {
            using (var reader = new StreamReader(this.fileSystem.File.Open(templateName, FileMode.Open, FileAccess.Read, FileShare.Read)))
            using (var writer = new StreamWriter(this.fileSystem.File.Open(outputFileHtml, FileMode.Create, FileAccess.Write, FileShare.None)))
            {
                var config = new TemplateServiceConfiguration();
                config.Language = Language.CSharp;
                config.EncodedStringFactory = new RawStringFactory();
                var service = RazorEngineService.Create(config);
                Engine.Razor = service;
                string processed = Engine.Razor.RunCompile(new LoadedTemplateSource(reader.ReadToEnd()), string.Empty, null, fileConfiguration);
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
            string pagePath = this.fileSystem.FileInfo.FromFileName(page.Path).FullName;
            using (StreamReader reader = new StreamReader(this.fileSystem.File.Open(pagePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
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
        /// The expected directory info list is the csproj's directory and all project references.
        /// </summary>
        /// <param name="csprojFile"></param>
        /// <returns></returns>
        private DirectoryInfoBase[] ExtractSourceDirectories(FileInfoBase csprojFile)
        {
            // Data validation
            Ensure.That(() => csprojFile).IsNotNull();
            Ensure.That(csprojFile.Exists, string.Format("Could not find '{0}' file", csprojFile)).IsTrue();

            // Initialize the extracted directories
            List<DirectoryInfoBase> extractedSourceDirectories = new List<DirectoryInfoBase>();

            // Add the csproj's directory
            DirectoryInfoBase projectDirectory = this.fileSystem.DirectoryInfo.FromDirectoryName(this.fileSystem.Path.GetDirectoryName(csprojFile.FullName));
            extractedSourceDirectories.Add(projectDirectory);

            // Load xml document
            XmlDocument xmlDocument = new XmlDocument();
            using (Stream stream = csprojFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                xmlDocument.Load(stream);
            }

            // Extract
            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
            xmlNamespaceManager.AddNamespace("msbuild", "http://schemas.microsoft.com/developer/msbuild/2003");
            XmlNodeList xmlNodes = xmlDocument.SelectNodes("//msbuild:ProjectReference", xmlNamespaceManager);
            for (int i = 0; i < xmlNodes.Count; ++i)
            {
                XmlNode xmlNode = xmlNodes.Item(i);
                string includeValue = xmlNode.Attributes["Include"].Value;
                string combinedPath = this.fileSystem.Path.Combine(projectDirectory.FullName, includeValue);

                // The combinedPath can contains both forward and backslash path chunk.
                // In linux environment we can end up having "/..\" in the path which make the GetDirectoryName method bugging (returns empty).
                // For this reason we need to make sure that the combined path uses forward slashes
                combinedPath = combinedPath.Replace(@"\", "/");

                // Add the combined path
                extractedSourceDirectories.Add(this.fileSystem.DirectoryInfo.FromDirectoryName(this.fileSystem.Path.GetDirectoryName(this.fileSystem.Path.GetFullPath(combinedPath))));
            }

            // Return the extracted directories
            return extractedSourceDirectories.ToArray();
        }
    }
}