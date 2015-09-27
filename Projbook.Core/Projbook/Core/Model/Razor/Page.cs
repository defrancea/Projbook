namespace Projbook.Core.Model.Razor
{
    /// <summary>
    /// Represents a documentation page.
    /// </summary>
    public class Page
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
        /// Whether the page is the home page.
        /// </summary>
        public bool IsHome { get; private set; }

        /// <summary>
        /// The page anchors.
        /// </summary>
        public Anchor[] Anchors { get; private set; }

        /// <summary>
        /// The page content.
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="Page"/>.
        /// </summary>
        /// <param name="id">Initializes the required <see cref="Id"/>.</param>
        /// <param name="title">Initializes the required <see cref="Title"/>.</param>
        /// <param name="isHome">Initializes the required <see cref="IsHome"/>.</param>
        /// <param name="anchor">Initializes the required <see cref="Anchors"/>.</param>
        /// <param name="content">Initializes the required <see cref="Content"/>.</param>
        public Page(string id, string title, bool isHome, Anchor[] anchor, string content)
        {
            this.Id = id;
            this.Title = title;
            this.IsHome = isHome;
            this.Anchors = anchor;
            this.Content = content;
        }
    }
}