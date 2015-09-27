using EnsureThat;
using System.IO;

namespace Projbook.Core.Model
{
    /// <summary>
    /// Represents a snippet rule for extracting content from code base.
    /// Todo: Right now CSharp is the only one parsed source file, but it's likely to happens we have different implementating for XML using xpath syntax.
    /// </summary>
    public class CSharpRule
    {
        /// <summary>
        /// The file from where the snipped is going to be extracted from.
        /// </summary>
        public FileInfo TargetFile { get; private set; }

        /// <summary>
        /// The target class.
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// The target method.
        /// </summary>
        public string MethodSignature { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="CSharpRule"/>.
        /// </summary>
        /// <param name="targetFile">Initializes the required <see cref="TargetFile"/>.</param>
        public CSharpRule(string targetFile)
        {
            // Data validation
            Ensure.That(() => targetFile).IsNotNullOrWhiteSpace();
            
            // Initialize and check
            this.TargetFile = new FileInfo(targetFile);
            Ensure.That(this.TargetFile.Exists, string.Format("Could not load '{0}' file because it doesn't exist.", this.TargetFile.FullName)).IsTrue();
        }
    }
}