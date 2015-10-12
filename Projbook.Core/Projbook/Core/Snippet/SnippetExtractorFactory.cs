using EnsureThat;
using Projbook.Core.Snippet.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

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
        /// Regex extracting the language and snippet extraction pattern.
        /// </summary>
        private static Regex regex = new Regex(@"^(.+)?\[(.+)\]$", RegexOptions.Compiled);

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
        }

        /// <summary>
        /// Creates a new instance of <see cref="ISnippetExtractor"/> according to the snippet extraction rule.
        /// </summary>
        /// <param name="snippetReference"></param>
        /// <returns>The matching snippet extractor. If </returns>
        public ISnippetExtractor CreateExtractor(string snippetReference)
        {
            // Return null if a code block doesn't have any details
            if (string.IsNullOrWhiteSpace(snippetReference))
            {
                return null;
            }

            // Match pattern
            Match match = regex.Match(snippetReference);
            if (match.Success)
            {
                // Retrieve code language and rule
                string language = match.Groups[1].Value;
                string pattern = match.Groups[2].Value;

                // Initialize the proper extracted depending on the language
                switch (language)
                {
                    case "csharp":
                        return new CSharpSnippetExtractor(language, pattern, this.ExtractSourceDirectories(this.CsprojFile));
                    default:
                        return new DefaultSnippetExtractor(language, pattern, this.ExtractSourceDirectories(this.CsprojFile));
                }
            }

            // No match found
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Extracts source directories from the csproj file.
        /// The extected directory info list is the csproj's directory and all project references.
        /// </summary>
        /// <param name="csprojFile"></param>
        /// <returns></returns>
        private DirectoryInfo[] ExtractSourceDirectories(FileInfo csprojFile)
        {
            // Data validation
            Ensure.That(() => csprojFile).IsNotNull();
            Ensure.That(csprojFile.Exists, string.Format("Could not find '{0}' file", csprojFile)).IsTrue();

            // Initialize the extracted directories
            List<DirectoryInfo> extractedSourceDirectories = new List<DirectoryInfo>();

            // Add the csproj's directory
            DirectoryInfo projectDirectory = new DirectoryInfo(Path.GetDirectoryName(csprojFile.FullName));
            extractedSourceDirectories.Add(projectDirectory);

            // Extract project reference path
            XNamespace msbuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";
            XDocument csprojDocument = XDocument.Load(csprojFile.FullName);
            IEnumerable<DirectoryInfo> referenceDirectories = csprojDocument
                .Element(msbuildNamespace + "Project")
                .Elements(msbuildNamespace + "ItemGroup")
                .Elements(msbuildNamespace + "ProjectReference")
                .Select(x => Path.GetDirectoryName(x.Attribute("Include").Value))
                .Select(x => new DirectoryInfo(Path.GetFullPath(Path.Combine(projectDirectory.FullName, x))));
            extractedSourceDirectories.AddRange(referenceDirectories);

            // Returne the extracted directories
            return extractedSourceDirectories.ToArray();
        }
    }
}