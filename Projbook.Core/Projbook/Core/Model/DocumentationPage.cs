using System.IO;

namespace Projbook.Core.Model
{
    /// <summary>
    /// Represents a documentation page.
    /// </summary>
    public class DocumentationPage
    {
        /// <summary>
        /// The page id.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// The page title.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// The page source file.
        /// </summary>
        public FileInfo File { get; private set; }

        /// <summary>
        /// The page index.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Whether the page is the home page.
        /// </summary>
        public bool IsHome { get; set; }

        /// <summary>
        /// The page anchors.
        /// </summary>
        public Anchor[] Anchors { get; set; }

        /// <summary>
        /// The page content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="DocumentationPage"/>.
        /// </summary>
        /// <param name="id">Initializes the required <see cref="Id"/>.</param>
        /// <param name="title">Initializes the required <see cref="Title"/>.</param>
        /// <param name="file">Initializes the required <see cref="File"/>.</param>
        /// <param name="index">Initializes the required <see cref="Index"/>.</param>
        public DocumentationPage(string id, string title, FileInfo file, int index)
        {
            this.Id = id;
            this.Title = title;
            this.File = file;
            this.Index = index;
        }
    }
}