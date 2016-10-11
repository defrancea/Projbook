using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Projbook.Core;
using Projbook.Core.Exception;
using Projbook.Core.Model;
using Projbook.Core.Model.Configuration;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;

namespace Projbook.Target
{
    /// <summary>
    /// Define MSBuild task triggering documentation generation.
    /// </summary>
    public class Projbook : Task
    {
        /// <summary>
        /// The project path.
        /// </summary>
        [Required]
        public string ProjectPath { get; set; }

        /// <summary>
        /// The extension path.
        /// </summary>
        [Required]
        public string ExtensionPath { get; set; }

        /// <summary>
        /// The configuration file.
        /// </summary>
        [Required]
        public string ConfigurationFile { get; set; }

        /// <summary>
        /// The output directory.
        /// </summary>
        [Required]
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Trigger task execution.
        /// </summary>
        /// <returns>True if the task succeeded.</returns>
        public override bool Execute()
        {
            // Load configuration
            FileSystem fileSystem = new FileSystem();
            ConfigurationLoader configurationLoader = new ConfigurationLoader(fileSystem);
            IndexConfiguration indexConfiguration;
            try
            {
                indexConfiguration = configurationLoader.Load(fileSystem.Path.GetDirectoryName(this.ProjectPath), this.ConfigurationFile);
            }
            catch (ConfigurationException configurationException)
            {
                // Report generation errors
                this.ReportErrors(configurationException.GenerationErrors);
                return false;
            }
            catch (Exception exception)
            {
                this.Log.LogError(string.Empty, string.Empty, string.Empty, this.ConfigurationFile, 0, 0, 0, 0, string.Format("Error during loading configuration: {0}", exception.Message));
                return false;
            }
            
            // Instantiate a ProjBook engine
            ProjbookEngine projbookEngine = new ProjbookEngine(fileSystem, this.ProjectPath, this.ExtensionPath, indexConfiguration, this.OutputDirectory);

            // Run generation
            bool success = true;
            GenerationError[] errors = projbookEngine.GenerateAll();

            // Report generation errors
            this.ReportErrors(errors);

            // Stop processing in case of error
            if (errors.Length > 0)
                success = false;

            return success;
        }

        /// <summary>
        /// Report generation errors.
        /// </summary>
        /// <param name="generationErrors">The generation errors to report.</param>
        private void ReportErrors(IEnumerable<GenerationError> generationErrors)
        {
            foreach (GenerationError generationError in generationErrors)
            {
                this.Log.LogError(string.Empty, string.Empty, string.Empty, generationError.SourceFile, generationError.Line, generationError.Column, 0, 0, generationError.Message);
            }
        }
    }
}