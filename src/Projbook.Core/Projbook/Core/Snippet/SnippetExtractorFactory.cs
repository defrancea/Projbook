using Projbook.Extension;
using Projbook.Extension.Spi;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.IO.Abstractions;
using System.Linq;
using System.Linq.Expressions;

namespace Projbook.Core.Snippet
{
    /// <summary>
    /// Creates the proper snippet exctractor depending on the pattern.
    /// </summary>
    public class SnippetExtractorFactory
    {
        /// <summary>
        /// Loaded extractor factories.
        /// </summary>
        Dictionary<string, Func<ISnippetExtractor>> loadedExtractorFactories = new Dictionary<string, Func<ISnippetExtractor>>();

        /// <summary>
        /// The default extractor factory.
        /// </summary>
        private Func<ISnippetExtractor> defaultExtractorFactory;

        /// <summary>
        /// The extension directory.
        /// </summary>
        private DirectoryInfoBase extensionDirectory;

        /// <summary>
        /// Initializes a new instance of <see cref="SnippetExtractorFactory"/>.
        /// </summary>
        /// <param name="extensionDirectory">The extension directory.</param>
        public SnippetExtractorFactory(DirectoryInfoBase extensionDirectory)
        {
            // Initialize
            this.defaultExtractorFactory = () => new DefaultSnippetExtractor();
            this.extensionDirectory = extensionDirectory;

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

            // Lookup factory in loaded extractor factories
            Func<ISnippetExtractor> matchingExtractorFactory;
            if (loadedExtractorFactories.TryGetValue(snippetExtractionRule.Language, out matchingExtractorFactory))
            {
                // Create instance from the matching extractor factory
                return matchingExtractorFactory();
            }

            // Create instance from default extractor factory
            return this.defaultExtractorFactory();
        }

        /// <summary>
        /// Loads extensions.
        /// </summary>
        private void LoadExtensions()
        {
            // If the plugin directory doesn't exist, abort plugin loading
            if (!this.extensionDirectory.Exists)
                return;

            // Initialize catalog
            AggregateCatalog catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new DirectoryCatalog(this.extensionDirectory.FullName));

            // Load ISnippetExtractor factories
            string metadataName = "ExportTypeIdentity";
            string targetTypeFullName = typeof(ISnippetExtractor).FullName;
            foreach (ComposablePartDefinition composablePartDefinition in catalog.Parts.AsEnumerable())
            {
                // Look for composable part definition being an ISnippetExtracttor
                if (composablePartDefinition.ExportDefinitions.Any(d =>
                    d.Metadata.ContainsKey(metadataName) &&
                    d.Metadata[metadataName].ToString() == targetTypeFullName))
                {
                    // Fetch the extension type from the composable part definition
                    Type extensionType = ReflectionModelServices.GetPartType(composablePartDefinition).Value;

                    // Try to retrieve syntax attribute
                    SyntaxAttribute syntax = Attribute.GetCustomAttribute(extensionType, typeof(SyntaxAttribute)) as SyntaxAttribute;
                    
                    // Add the extractor type to loaded ones if a syntax attribute is found and the type has a default contructor
                    if (null != syntax && null != extensionType.GetConstructor(Type.EmptyTypes))
                    {
                        // Create extractor factory
                        Func<ISnippetExtractor> extractorFactory = Expression.Lambda<Func<ISnippetExtractor>>(Expression.New(extensionType)).Compile();

                        // Record the created fectory as a loaded one
                        this.loadedExtractorFactories.Add(syntax.Name, extractorFactory);
                    }
                }
            }

            // Lookup in loaded extractors to detect custom default extractor
            Func<ISnippetExtractor> customDefaultExtractorFactory;
            if (this.loadedExtractorFactories.TryGetValue("*", out customDefaultExtractorFactory))
            {
                // Set the custom default extractor
                this.defaultExtractorFactory = customDefaultExtractorFactory;
            }
        }
    }
}