using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using EnsureThat;
using Projbook.Core.Exception;
using Projbook.Core.Projbook.Core.Snippet.CSharp;

namespace Projbook.Core.Snippet
{
    /// <summary>
    /// Extractor in charge of browsing source directories. load file content and extract requested member.
    /// </summary>
    public class DefaultSnippetExtractor : ISnippetExtractor
    {
        /// <summary>
        /// All source directories where snippets could possibly be.
        /// </summary>
        public DirectoryInfo[] SourceDictionaries { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultSnippetExtractor"/>.
        /// </summary>
        /// <param name="sourceDirectories">Initializes the required <see cref="SourceDictionaries"/>.</param>
        public DefaultSnippetExtractor(params DirectoryInfo[] sourceDirectories)
        {
            // Data validation
            Ensure.That(() => sourceDirectories).IsNotNull();
            Ensure.That(() => sourceDirectories).HasItems();

            // Initialize
            this.SourceDictionaries = sourceDirectories;
        }

        /// <summary>
        /// Extracts a snippet.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="pattern">The extraction pattern, never used for this implementation.</param>
        /// <returns>The extracted snippet.</returns>
        public virtual Model.Snippet Extract(string filePath, string pattern)
        {
            // Extract file content
            string sourceCode = this.LoadFile(filePath);

            // Return the entire code
            return this.BuildSnippet(sourceCode);
        }
        
        /// <summary>
        /// Loads a file from the file name.
        /// </summary>
        /// <param name="filePath">The file path to load.</param>
        /// <returns>The file's content.</returns>
        protected string LoadFile(string filePath)
        {
            // Look for the file in available source directories
            FileInfo fileInfo = null;
            foreach (DirectoryInfo directoryInfo in this.SourceDictionaries)
            {
                string fullFilePath = Path.Combine(directoryInfo.FullName, filePath ?? string.Empty);
                if (File.Exists(fullFilePath))
                {
                    fileInfo = new FileInfo(fullFilePath);
                    break;
                }
            }

            // Raise an error if cannot find the file
            if (null == fileInfo)
            {
                throw new SnippetExtractionException("Cannot find file in any referenced project", filePath);
            }

            // Load the file content
            MemoryStream memoryStream = new MemoryStream();
            using (var fileReader = new StreamReader(new FileStream(fileInfo.FullName, FileMode.Open)))
            using (var fileWriter = new StreamWriter(memoryStream))
            {
                fileWriter.Write(fileReader.ReadToEnd());
            }

            // Read the code snippet from the file
            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        /// <summary>
        /// Builds a snippet from a full file content.
        /// </summary>
        /// <param name="fileContent">The file content.</param>
        /// <returns>The built snippet.</returns>
        private Model.Snippet BuildSnippet(string fileContent)
        {
            // Data validation
            Ensure.That(() => fileContent).IsNotNull();

            // Extract each lines
            StringBuilder stringBuilder = new StringBuilder();
            List<string> lines = new List<string>();
            using (StringReader stringReader = new StringReader(fileContent))
            {
                while (true)
                {
                    string line = stringReader.ReadLine();
                    if (null != line)
                    {
                        lines.Add(line);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // Write the snippet
            this.WriteAndCleanupSnippet(stringBuilder, lines.ToArray(), CSharpExtractionMode.FullMember);

            // Create the snippet from the exctracted code
            return new Model.Snippet(stringBuilder.ToString());
        }

        /// <summary>
        /// Writes and cleanup line snippets.
        /// Snippets are moved out of their context, for this reasong we need to trim lines aroung and remove a part of the indentation.
        /// </summary>
        /// <param name="stringBuilder">The string builder used as output.</param>
        /// <param name="lines">The lines to process.</param>
        /// <param name="extractionMode">The extraction mode.</param>
        private void WriteAndCleanupSnippet(StringBuilder stringBuilder, string[] lines, CSharpExtractionMode extractionMode)
        {
            // Data validation
            Ensure.That(() => stringBuilder).IsNotNull();
            Ensure.That(() => lines).IsNotNull();

            // Do not process if lines are empty
            if (0 >= lines.Length)
            {
                return;
            }

            // Compute the index of the first selected line
            int startPos = 0;
            if (CSharpExtractionMode.ContentOnly == extractionMode)
            {
                for (; startPos < lines.Length && !lines[startPos].ToString().Contains('{'); ++startPos) ;

                // Extract block code if any opening bracket has been found
                if (startPos < lines.Length)
                {
                    int openingBracketPos = lines[startPos].IndexOf('{');
                    if (openingBracketPos >= 0)
                    {
                        // Extract the code before the curly bracket
                        if (lines[startPos].Length > openingBracketPos)
                        {
                            lines[startPos] = lines[startPos].Substring(openingBracketPos + 1);
                        }

                        // Skip the current line if empty
                        if (string.IsNullOrWhiteSpace(lines[startPos]) && lines.Length > 1 + startPos)
                        {
                            ++startPos;
                        }
                    }
                }
            }
            else
            {
                for (; startPos < lines.Length && lines[startPos].ToString().Trim().Length == 0; ++startPos) ;
            }

            // Compute the index of the lastselected line
            int endPos = -1 + lines.Length;
            if (CSharpExtractionMode.ContentOnly == extractionMode)
            {
                for (; 0 <= endPos && !lines[endPos].ToString().Contains('}'); --endPos) ;

                // Extract block code if any closing bracket has been found
                if (0 <= endPos)
                {
                    int closingBracketPos = lines[endPos].IndexOf('}');
                    if (closingBracketPos >= 0)
                    {
                        // Extract the code before the curly bracket
                        if (lines[endPos].Length > closingBracketPos)
                            lines[endPos] = lines[endPos].Substring(0, closingBracketPos).TrimEnd();
                    }

                    // Skip the current line if empty
                    if (string.IsNullOrWhiteSpace(lines[endPos]) && lines.Length > -1 + endPos)
                    {
                        --endPos;
                    }
                }
            }
            else
            {
                for (; 0 <= endPos && lines[endPos].ToString().Trim().Length == 0; --endPos) ;
            }

            // Compute the padding to remove for removing a part of the indentation
            int leftPadding = int.MaxValue;
            for (int i = startPos; i <= endPos; ++i)
            {
                // Ignore empty lines in the middle of the snippet
                if (!string.IsNullOrWhiteSpace(lines[i]))
                {
                    // Adjust the left padding with the available whitespace at the beginning of the line
                    leftPadding = Math.Min(leftPadding, lines[i].ToString().TakeWhile(Char.IsWhiteSpace).Count());
                }
            }

            // Write selected lines to the string builder
            bool firstLine = true;
            for (int i = startPos; i <= endPos; ++i)
            {
                // Write line return between each line
                if (!firstLine)
                {
                    stringBuilder.AppendLine();
                }

                // Remove a part of the indentation padding
                if (lines[i].Length > leftPadding)
                {
                    string line = lines[i].Substring(leftPadding);

                    // Process the snippet depending on the extraction mode
                    switch (extractionMode)
                    {
                        // Extract the block structure only
                        case CSharpExtractionMode.BlockStructureOnly:
                            int openingBracketPos = line.IndexOf('{');
                            if (openingBracketPos >= 0)
                            {
                                // Extract the code before the curly bracket
                                if (line.Length > openingBracketPos)
                                    line = line.Substring(0, 1 + openingBracketPos);

                                // Replace the content and close the block
                                line += string.Format("{0}    // ...{0}}}", Environment.NewLine);

                                // Stop the iteration
                                endPos = i;
                            }
                            break;
                    }

                    // Append the line
                    stringBuilder.Append(line);
                }

                // Flag the first line as false
                firstLine = false;
            }
        }
    }
}