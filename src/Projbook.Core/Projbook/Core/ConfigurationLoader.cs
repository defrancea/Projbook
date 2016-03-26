using EnsureThat;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Projbook.Core.Exception;
using Projbook.Core.Model.Configuration;
using Projbook.Core.Projbook.Core.Model.Configuration.Validation;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Projbook.Core
{
    /// <summary>
    /// Responsible for loading configuration.
    /// </summary>
    public class ConfigurationLoader
    {
        /// <summary>
        /// Json deserialization error line and column extractor.
        /// </summary>
        private static Regex errorLocationExtractor = new Regex(@"line (\d+), position (\d+)", RegexOptions.Compiled);

        /// <summary>
        /// Load the configuraiton
        /// </summary>
        /// <param name="projectLocation">The project location.</param>
        /// <param name="configurationFile">The configuration to load.</param>
        /// <returns>The loaded configuration.</returns>
        public Configuration[] Load(string projectLocation, string configurationFile)
        {
            // Data validation
            Ensure.That(() => projectLocation).IsNotNull();
            Ensure.That(() => configurationFile).IsNotNull();
            Ensure.That(Directory.Exists(projectLocation), string.Format("Could not find '{0}': Directory not found", projectLocation)).IsTrue();

            // Compute and validate configuration path
            string configurationPath = Path.Combine(projectLocation, configurationFile);
            Ensure.That(File.Exists(configurationPath), string.Format("Could not load configuration '{0}': File not found", configurationPath)).IsTrue();
            
            // Deserialize configuration
            Configuration[] configurations;
            string content = string.Empty;
            using (var reader = new StreamReader(new FileStream(configurationPath, FileMode.Open, FileAccess.Read)))
            {
                // Read the content
                content = reader.ReadToEnd();

                // Deserialize as contiguration array if the configuration content looks containing many generation configuraiton
                try
                {
                    // Prepare configuration parsing
                    JsonTextReader jsonReader = new JsonTextReader(new StringReader(content));
                    JSchemaValidatingReader jsonValidatingReader = new JSchemaValidatingReader(jsonReader);
                    JsonSerializer jsonSerializer = new JsonSerializer();

                    // Parse the configuration against the right schema
                    if (content.TrimStart().StartsWith("["))
                    {
                        // Load schema
                        JSchema schemaJson = JSchema.Parse(ConfigurationValidation.ConfigurationArraySchema);
                        jsonValidatingReader.Schema = schemaJson;

                        // Deserialize condiguration
                        configurations = jsonSerializer.Deserialize<Configuration[]>(jsonValidatingReader);
                    }

                    // Otherwise Deserialize as simple configuration
                    else
                    {
                        // Load schema
                        JSchema schemaJson = JSchema.Parse(ConfigurationValidation.ConfigurationSchema);
                        jsonValidatingReader.Schema = schemaJson;
                        
                        // Deserialize condiguration
                        configurations = new Configuration[] { jsonSerializer.Deserialize<Configuration>(jsonValidatingReader) };
                    }
                }

                // Report validation exception
                catch (JSchemaValidationException validationException)
                {
                    throw new ConfigurationException(new Model.GenerationError(configurationPath, validationException.Message, validationException.LineNumber, validationException.LinePosition));
                }

                // Report serialization exception
                catch (System.Exception exception)
                {
                    // Try to extract error line and column
                    Match match = ConfigurationLoader.errorLocationExtractor.Match(exception.Message);

                    // Initialize line and column to 0 which is the default value when the message match is not possible
                    int line = 0;
                    int column = 0;

                    // If the match is successful, update line and column value
                    if (match.Success)
                    {
                        line = int.Parse(match.Groups[1].Value);
                        column = int.Parse(match.Groups[2].Value) + 1; // +1 because the reported line by the Deserialize method is 0 based but MSBuild is 1 based
                    }

                    // Throw the configuration exception with line and column data
                    throw new ConfigurationException(new Model.GenerationError(configurationPath, exception.Message, line, column));
                }
            }

            // Ensure valid references
            HashSet<string> htmlTemplateNotFound = new HashSet<string>();
            HashSet<string> pdfTemplateNotFound = new HashSet<string>();
            HashSet<string> pageNotFound = new HashSet<string>();
            foreach (Configuration configuration in configurations)
            {
                // Resolve html template path
                string originalHtmlTemplateValue = configuration.TemplateHtml;
                if (!string.IsNullOrWhiteSpace(configuration.TemplateHtml))
                {
                    configuration.TemplateHtml = Path.Combine(projectLocation, configuration.TemplateHtml);
                }

                // Resolve pdf template path
                string originalPdfTemplateValue = configuration.TemplatePdf;
                if (!string.IsNullOrWhiteSpace(configuration.TemplatePdf))
                {
                    configuration.TemplatePdf = Path.Combine(projectLocation, configuration.TemplatePdf);
                }

                // Detect if generation is enabled and validate the file
                if (!string.IsNullOrWhiteSpace(configuration.TemplateHtml) && !File.Exists(configuration.TemplateHtml))
                {
                    htmlTemplateNotFound.Add(originalHtmlTemplateValue);
                }
                if (!string.IsNullOrWhiteSpace(configuration.TemplatePdf) && !File.Exists(configuration.TemplatePdf))
                {
                    pdfTemplateNotFound.Add(originalPdfTemplateValue);
                }

                // Set default out file name
                if (!string.IsNullOrWhiteSpace(configuration.TemplateHtml) && string.IsNullOrWhiteSpace(configuration.OutputHtml))
                {
                    configuration.OutputHtml = this.InjectSuffix(configuration.TemplateHtml, "generated");
                }
                if (!string.IsNullOrWhiteSpace(configuration.TemplatePdf) && string.IsNullOrWhiteSpace(configuration.OutputPdf))
                {
                    configuration.OutputPdf = this.InjectSuffix(configuration.TemplatePdf, "generated");
                }

                // Initialize pages to empty array if none are defined
                if (null == configuration.Pages)
                {
                    configuration.Pages = new Page[0];
                }

                // Ensure that each pages exists
                else
                {
                    foreach (Page page in configuration.Pages)
                    {
                        string pagePath = Path.Combine(projectLocation, page.Path);
                        if (File.Exists(page.Path))
                        {
                            page.FileSystemPath = pagePath;
                        }
                        else
                        {
                            pageNotFound.Add(page.Path);
                        }
                    }
                }
            }

            // Build generation errors
            List<Model.GenerationError> generationErrors = new List<Model.GenerationError>();

            // Process html template not found error
            if (htmlTemplateNotFound.Count > 0)
            {
                generationErrors.AddRange(this.ComputeErrors(configurationPath, content, "Could not find html template '{0}'", @"""template-html"":\s*""({0})""", htmlTemplateNotFound));
            }

            // Process pdf template not found error
            if (pdfTemplateNotFound.Count > 0)
            {
                generationErrors.AddRange(this.ComputeErrors(configurationPath, content, "Could not find html template '{0}'", @"""template-pdf"":\s*""({0})""", pdfTemplateNotFound));
            }

            // Process pages not found error
            if (pageNotFound.Count > 0)
            {
                generationErrors.AddRange(this.ComputeErrors(configurationPath, content, "Could not find page '{0}'", @"""path"":\s*""({0})""", pageNotFound));
            }

            // Raise exception if the configuration contains some error
            if (generationErrors.Count > 0)
            {
                throw new ConfigurationException(generationErrors.ToArray());
            }
            
            // Return the configuration
            return configurations;
        }

        /// <summary>
        /// Injects a sufix right before the extension.
        /// </summary>
        /// <param name="filename">The file name.</param>
        /// <param name="suffix">The suffix.</param>
        /// <returns>The processed file name.</returns>
        private string InjectSuffix(string filename, string suffix)
        {
            // Data validation
            Ensure.That(() => filename).IsNotNullOrWhiteSpace();
            Ensure.That(() => suffix).IsNotNullOrWhiteSpace();

            // Inject the suffix
            return string.Format("{0}-{1}{2}", Path.GetFileNameWithoutExtension(filename), suffix, Path.GetExtension(filename));
        }

        /// <summary>
        /// Locates and computes line and column where errors are located in the configuration.
        /// This implementation could be optimized a lot considering because of the usage of dynamic regex + n2 algorithm.
        /// However, considering this code is trigerred for error reporting and the usual size of configurations is really small, this implementation is acceptable.
        /// </summary>
        /// <param name="filename">The file name.</param>
        /// <param name="content">The content from where checking the position.</param>
        /// <param name="errorMessage">The error message to report.</param>
        /// <param name="regexTemplate">The regex template.</param>
        /// <param name="references">The reference to locate.</param>
        /// <returns>The errors.</returns>
        private IEnumerable<Model.GenerationError> ComputeErrors(string filename, string content, string errorMessage, string regexTemplate, IEnumerable<string> references)
        {
            // Data validation
            Ensure.That(() => filename).IsNotNullOrWhiteSpace();
            Ensure.That(() => content).IsNotNullOrWhiteSpace();
            Ensure.That(() => errorMessage).IsNotNullOrWhiteSpace();
            Ensure.That(() => regexTemplate).IsNotNullOrWhiteSpace();

            // Process each reference
            foreach (string reference in references)
            {
                // Match and process each matching
                Regex regex = new Regex(string.Format(regexTemplate, reference));
                foreach (Match match in regex.Matches(content))
                {
                    // Initialize the line and lastLine index to 1 as default value
                    int line = 1;
                    int lastLineIndex = 0;

                    // Scan each char from 0 to the matching index
                    // This part can be optimized by selecting the highest index and keep track of each intermediate index found on the way
                    // It doesn't really worth optimizing this part because configurations cannot be big enough for being a performence issue here
                    for (int i = 0; i < match.Groups[1].Index; ++i)
                    {
                        // Every time we meet a line return, increment the line count and save the last line index to compute column position
                        if ('\n' == content[i])
                        {
                            ++line;
                            lastLineIndex = 1 + i;
                        }
                    }

                    // Compute column position being the matching index - the latest line break + 1 (configurations are 1 based)
                    int column = match.Groups[1].Index - lastLineIndex + 1;

                    // Produce the error
                    yield return new Model.GenerationError(filename, string.Format(errorMessage, reference), line, column);
                }
            }
        }
    }
}