using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Projbook.Core;

namespace Projbook.Target
{
    /// <summary>
    /// Define MSBuild task trigerring documentation generation.
    /// </summary>
    public class Projbook : Task
    {
        /// <summary>
        /// The solution directory.
        /// </summary>
        [Required]
        public string SolutionDirectory { get; set; }

        /// <summary>
        /// The source directory where the snippets are located.
        /// </summary>
        [Required]
        public string SourceDirectory { get; set; }

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
            ProjbookEngine projbookEngine = new ProjbookEngine(this.SolutionDirectory, this.SourceDirectory, this.TemplateFile, this.ConfigurationFile, this.OutputDirectory);
            projbookEngine.Generate();

            // Return output
            return true;
        }
    }
}