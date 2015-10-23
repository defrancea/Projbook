using EnsureThat;

namespace Projbook.Core.Model
{
    /// <summary>
    /// Represents a generation error.
    /// </summary>
    public class GenerationError
    {
        /// <summary>
        /// The file for which the error happened.
        /// </summary>
        public string SourceFile { get; private set; }

        /// <summary>
        /// The error message.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="ProjbookEngine"/>.
        /// </summary>
        /// <param name="sourceFile">Initializes the required <see cref="SourceFile"/>.</param>
        /// <param name="message">Initializes the required <see cref="Message"/>.</param>
        public GenerationError(string sourceFile, string message)
        {
            // Data validation
            Ensure.That(() => sourceFile).IsNotNullOrWhiteSpace();
            Ensure.That(() => message).IsNotNullOrWhiteSpace();

            // Initialize
            this.SourceFile = sourceFile;
            this.Message = message;
        }
    }
}