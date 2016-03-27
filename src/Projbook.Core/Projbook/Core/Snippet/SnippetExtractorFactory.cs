using EnsureThat;
using Projbook.Extension;
using Projbook.Extension.Spi;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.IO;
using System.Linq;

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
        /// Loaded extractor types.
        /// </summary>
        Dictionary<string, Type> loadedExtractorTypes = new Dictionary<string, Type>();

        /// <summary>
        /// The default extractor type.
        /// </summary>
        private Type defaultExtractorType;

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
            this.defaultExtractorType = typeof(DefaultSnippetExtractor);

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

            // Lookup type in loaded extractors
            Type matchingExtractorType;
            if (loadedExtractorTypes.TryGetValue(snippetExtractionRule.Language, out matchingExtractorType))
            {
                return Activator.CreateInstance(matchingExtractorType) as ISnippetExtractor;
            }

            // Return default extractor
            return Activator.CreateInstance(defaultExtractorType) as ISnippetExtractor;
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
            
            // Load ISnippetExtractor types
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
                        this.loadedExtractorTypes.Add(syntax.Name, extensionType);
                    }
                }
            }

            // Lookup in loaded extractors to detect custom default extractor
            Type customDefaultExtractorType;
            if (this.loadedExtractorTypes.TryGetValue("*", out customDefaultExtractorType))
            {
                // Set the custom default extractor
                this.defaultExtractorType = customDefaultExtractorType;
            }
        }
    }
}