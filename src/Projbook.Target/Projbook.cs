using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Projbook.Core;
using Projbook.Core.Model;

namespace Projbook.Target
{
    /// <summary>
    /// Define MSBuild task trigerring documentation generation.
    /// </summary>
    public class Projbook : Task
    {
        /// <summary>
        /// The project path.
        /// </summary>
        [Required]
        public string ProjectPath { get; set; }

        /// <summary>
        /// The template file.
        /// </summary>
        [Required]
        public string TemplateFile { get; set; }

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
            // Run generation
            ProjbookEngine projbookEngine = new ProjbookEngine(this.ProjectPath, this.TemplateFile, this.ConfigurationFile, this.OutputDirectory);
            GenerationError[] errors = projbookEngine.Generate();

            // Report generation errors
            foreach (GenerationError error in errors)
            {
                this.Log.LogError(string.Empty, string.Empty, string.Empty, error.SourceFile, -1, -1, -1, -1, error.Message);
            }

            // Return output
            return errors.Length <= 0;
        }
    }
}