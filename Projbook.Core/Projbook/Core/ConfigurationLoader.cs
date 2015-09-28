using EnsureThat;
using Newtonsoft.Json;
using Projbook.Core.Model.Configuration;
using System.IO;

namespace Projbook.Core.Projbook.Core
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
        public Configuration Load(FileInfo configurationFile)
        {
            // Data validation
            Ensure.That(() => configurationFile).IsNotNull();
            Ensure.That(configurationFile.Exists, string.Format("Could not load '{0}': File not found", configurationFile.FullName)).IsTrue();

            // Deserialize configuration
            Configuration configuration;
            using (var reader = new StreamReader(new FileStream(configurationFile.FullName, FileMode.Open)))
            {
                configuration = JsonConvert.DeserializeObject<Configuration>(reader.ReadToEnd());
            }

            // Initialize page with empty array if none are found
            if (null == configuration.Pages)
            {
                configuration.Pages = new Page[0];
            }

            // Return the configuration
            return configuration;
        }
    }
}