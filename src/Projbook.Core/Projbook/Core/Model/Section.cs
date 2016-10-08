using EnsureThat;

namespace Projbook.Core.Model
{
    /// <summary>
    /// Represents a documentation section.
    /// </summary>
    public class Section
    {
        /// <summary>
        /// The section id.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// The section level.
        /// </summary>
        public int Level { get; private set; }

        /// <summary>
        /// The section title.
        /// </summary>
        public string Title { get; private set; }
        
        /// <summary>
        /// The section content.
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="Page"/>.
        /// </summary>
        /// <param name="id">Initializes the required <see cref="Id"/>.</param>
        /// <param name="level">Initializes the required <see cref="Level"/>.</param>
        /// <param name="title">Initializes the required <see cref="Title"/>.</param>
        /// <param name="content">Initializes the required <see cref="Content"/>.</param>
        public Section(string id, int level, string title, string content)
        {
            // Data validation
            Ensure.That(() => id).IsNotNullOrWhiteSpace();
            Ensure.That(() => level).IsGte(0);
            Ensure.That(() => title).IsNotNullOrWhiteSpace();
            Ensure.That(() => content).IsNotNullOrWhiteSpace();

            // Initialize
            this.Id = id;
            this.Level = level;
            this.Title = title;
            this.Content = content;
        }
    }
}