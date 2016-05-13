namespace Projbook.Core.Model.Configuration
{
    /// <summary>
    /// Represents a document index configuration.
    /// </summary>
    public class IndexConfiguration
    {
        /// <summary>
        /// The index title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The index description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The index template.
        /// </summary>
        public string Template { get; set; }

        /// <summary>
        /// The index output.
        /// </summary>
        public string Output { get; set; }

        /// <summary>
        /// The index output.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// The index configurations.
        /// </summary>
        public Configuration[] Configurations { get; set; }
    }
}