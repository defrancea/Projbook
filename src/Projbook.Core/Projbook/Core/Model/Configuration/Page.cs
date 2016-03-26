namespace Projbook.Core.Model.Configuration
{
    /// <summary>
    /// Represents a page.
    /// </summary>
    public class Page
    {
        /// <summary>
        /// The page path.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The page file system path.
        /// </summary>
        public string FileSystemPath { get; set; }

        /// <summary>
        /// The page title.
        /// </summary>
        public string Title { get; set; }
    }
}