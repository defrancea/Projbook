using EnsureThat;
using System;

namespace Projbook.Extension.Spi
{
    /// <summary>
    /// Defines supported syntax by a <see cref="ISnippetExtractor"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SyntaxAttribute : Attribute
    {
        /// <summary>
        /// The syntax name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="SyntaxAttribute"/>.
        /// </summary>
        /// <param name="name">The syntax name.</param>
        public SyntaxAttribute(string name)
        {
            // Data validation
            Ensure.That(() => name).IsNotNull();

            // Initialize
            this.Name = name;
        }
    }
}