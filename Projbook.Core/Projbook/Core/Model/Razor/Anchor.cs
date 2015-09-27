using EnsureThat;

namespace Projbook.Core.Model.Razor
{
    /// <summary>
    /// Represents an anchor in a document.
    /// </summary>
    public class Anchor
    {
        /// <summary>
        /// The anchor label corresponding to an header.
        /// </summary>
        public string Label { get; private set; }

        /// <summary>
        /// The anchor lebel.
        /// </summary>
        public int Level { get; private set; }

        /// <summary>
        /// The anchor value that could be used as hyperlink target in order to jump to the section.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="Anchor"/>.
        /// </summary>
        /// <param name="label">Initializes the required <see cref="Label"/>.</param>
        /// <param name="level">Initializes the required <see cref="Level"/>.</param>
        /// <param name="value">Initializes the required <see cref="Value"/>.</param>
        public Anchor(string label, int level, string value)
        {
            // Data validation
            Ensure.That(() => label).IsNotNullOrWhiteSpace();
            Ensure.That(() => level).IsGte(0);
            Ensure.That(() => value).IsNotNullOrWhiteSpace();

            // Initializes
            this.Label = label;
            this.Level = level;
            this.Value = value;
        }
    }
}