using EnsureThat;
using Projbook.Extension;
using Projbook.Extension.Spi;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;

namespace Projbook.Core.Snippet
{
    /// <summary>
    /// Creates the proper snippet exctractor depending on the pattern.
    /// </summary>
    public class SnippetExtractorFactory
    {
        /// <summary>
        /// The csproj file.
        /// </summary>
        public FileInfo CsprojFile { get; private set; }

        /// <summary>
        /// Discovered extractors filled by MEF during extension loading.
        /// </summary>
        [ImportMany]
        private IEnumerable<ISnippetExtractor> discoveredExtractors;

        /// <summary>
        /// Loaded extractors.
        /// </summary>
        Dictionary<string, ISnippetExtractor> loadedExtractors = new Dictionary<string, ISnippetExtractor>();

        /// <summary>
        /// The default extractor.
        /// </summary>
        private ISnippetExtractor defaultExtractor;

        /// <summary>
        /// Initializes a new instance of <see cref="SnippetExtractorFactory"/>.
        /// </summary>
        /// <param name="csprojFile">Initializes the required <see cref="SourceDictionaries"/>.</param>
        public SnippetExtractorFactory(FileInfo csprojFile)
        {
            // Data validation
            Ensure.That(() => csprojFile).IsNotNull();
            Ensure.That(csprojFile.Exists, string.Format("Could not find '{0}' file", csprojFile)).IsTrue();

            // Initialize
            this.CsprojFile = csprojFile;
            this.defaultExtractor = new DefaultSnippetExtractor();

            // Load extensions
            this.LoadExtensions();
        }
        
        /// <summary>
        /// Creates a new instance of <see cref="ISnippetExtractor"/> according to the snippet extraction rule.
        /// </summary>
        /// <param name="snippetExtractionRule">The snippet extraction rule.</param>
        /// <returns>The matching snippet extractor.</returns>
        public ISnippetExtractor CreateExtractor(SnippetExtractionRule snippetExtractionRule)
        {
            // Return null if the extraction rule is null
            if (null == snippetExtractionRule)
            {
                return null;
            }

            // Lookup in loaded extractors
            ISnippetExtractor matchingExtractor;
            if (loadedExtractors.TryGetValue(snippetExtractionRule.Language, out matchingExtractor))
            {
                return matchingExtractor;
            }

            // Return default extractor
            return this.defaultExtractor;
        }

        /// <summary>
        /// Loads extensions.
        /// </summary>
        private void LoadExtensions()
        {
            // Initialize container
            DirectoryCatalog directoryCatalog = new DirectoryCatalog(".");
            AggregateCatalog catalog = new AggregateCatalog(directoryCatalog);
            CompositionContainer container = new CompositionContainer(catalog);

            // Load extensions
            container.ComposeParts(this);

            // Determine syntax of discovered extractors
            foreach (ISnippetExtractor extractor in this.discoveredExtractors)
            {
                // Try to retrieve syntax attribute
                SyntaxAttribute syntax = Attribute.GetCustomAttribute(extractor.GetType(), typeof(SyntaxAttribute)) as SyntaxAttribute;

                // Add the extractor to loaded extractors if a syntax attribute is found
                if (null != syntax)
                {
                    loadedExtractors.Add(syntax.Name, extractor);
                }
            }

            // Lookup in loaded extractors to detect custom default extractor
            ISnippetExtractor customDefaultExtractor;
            if (loadedExtractors.TryGetValue("*", out customDefaultExtractor))
            {
                // Set the custom default extractor
                this.defaultExtractor = customDefaultExtractor;
            }
        }
    }
}