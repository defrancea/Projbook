using EnsureThat;

namespace Projbook.Core.Model
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
        /// The pre section content.
        /// </summary>
        public string PreSectionContent { get; private set; }

        /// <summary>
        /// The page content.
        /// </summary>
        public Section[] Sections { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="Page"/>.
        /// </summary>
        /// <param name="id">Initializes the required <see cref="Id"/>.</param>
        /// <param name="title">Initializes the required <see cref="Title"/>.</param>
        /// <param name="preSectionContent">Initializes the required <see cref="PreSectionContent"/>.</param>
        /// <param name="sections">Initializes the required <see cref="Sections"/>.</param>
        public Page(string id, string title, string preSectionContent, Section[] sections)
        {
            // Data validation
            Ensure.That(() => id).IsNotNullOrWhiteSpace();
            Ensure.That(() => title).IsNotNullOrWhiteSpace();
            Ensure.That(() => preSectionContent).IsNotNull();
            Ensure.That(() => sections).IsNotNull();

            // Initialize
            this.Id = id;
            this.Title = title;
            this.PreSectionContent = preSectionContent;
            this.Sections = sections;
        }
    }
}