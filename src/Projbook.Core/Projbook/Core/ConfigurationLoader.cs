using EnsureThat;
using Newtonsoft.Json;
using Projbook.Core.Model.Configuration;
using System.IO;

namespace Projbook.Core
{
    /// <summary>
    /// Responsible for loading configuration.
    /// </summary>
    public class ConfigurationLoader
    {
        /// <summary>
        /// Load the configuraiton
        /// </summary>
        /// <param name="configurationFile">The configuration to load.</param>
        /// <returns>The loaded configuration.</returns>
        public Configuration[] Load(FileInfo configurationFile)
        {
            // Data validation
            Ensure.That(() => configurationFile).IsNotNull();
            Ensure.That(configurationFile.Exists, string.Format("Could not load '{0}': File not found", configurationFile.FullName)).IsTrue();

            // Deserialize configuration
            Configuration[] configurations;
            using (var reader = new StreamReader(new FileStream(configurationFile.FullName, FileMode.Open)))
            {
                // Read the content
                string content = reader.ReadToEnd();

                // Deserialize as contiguration array if the configuration content looks containing many generation configuraiton
                if (content.TrimStart().StartsWith("["))
                {
                    configurations = JsonConvert.DeserializeObject<Configuration[]>(content);

                    if (configurations.Length <= 0)
                    {
                        throw new System.Exception("Could not find configuration definition");
                    }
                }

                // Otherwise Deserialize as simple configuration
                else
                {
                    configurations = new Configuration[] { JsonConvert.DeserializeObject<Configuration>(content) };
                }
            }

            // Ensure valid pages
            foreach (Configuration configuration in configurations)
            {
                // Validate that at least one template is definied
                if (string.IsNullOrWhiteSpace(configuration.TemplateHtml) && string.IsNullOrWhiteSpace(configuration.TemplatePdf))
                {
                    throw new System.Exception("At least one template must to be definied, possible template configuration: template-html, template-pdf");
                }

                // Detect if generation is enabled and validate the file
                if (!string.IsNullOrWhiteSpace(configuration.TemplateHtml) && !File.Exists(configuration.TemplateHtml))
                {
                    throw new System.Exception(string.Format("Could not find template '{0}'", configuration.TemplateHtml));
                }
                if (!string.IsNullOrWhiteSpace(configuration.TemplatePdf) && !File.Exists(configuration.TemplatePdf))
                {
                    throw new System.Exception(string.Format("Could not find template '{0}'", configuration.TemplatePdf));
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
                        if (!File.Exists(page.Path))
                        {
                            throw new System.Exception(string.Format("Could not find page '{0}'", page.Path));
                        }
                    }
                }
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
    }
}