﻿using EnsureThat;

namespace Projbook.Core.Markdown
{
    /// <summary>
    /// Represents page break info.
    /// </summary>
    public class PageBreakInfo
    {
        /// <summary>
        /// The page break id.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// The page break level.
        /// </summary>
        public int Level { get; private set; }

        /// <summary>
        /// The page break title.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// The page break position.
        /// </summary>
        public long Position { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="PageBreakInfo"/>.
        /// </summary>
        /// <param name="id">Initializes the required <see cref="CsprojFile"/>.</param>
        /// <param name="level">Initializes the required <see cref="Level"/>.</param>
        /// <param name="title">Initializes the required <see cref="Title"/>.</param>
        /// <param name="position">Initializes the required <see cref="Position"/>.</param>
        public PageBreakInfo(string id, int level, string title, long position)
        {
            // Data validation
            Ensure.That(() => id).IsNotNullOrWhiteSpace();
            Ensure.That(() => level).IsGte(0);
            Ensure.That(() => title).IsNotNull();
            Ensure.That(() => position).IsGte(0);

            // Initialize
            this.Id = id;
            this.Level = level;
            this.Title = title;
            this.Position = position;
        }
    }
}