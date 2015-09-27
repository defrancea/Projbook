namespace Projbook.Core.Model.Configuration
{
    /// <summary>
    /// Represents a document configuration.
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// The document title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Configuration pages.
        /// </summary>
        public Page[] Pages { get; set; }
    }
}