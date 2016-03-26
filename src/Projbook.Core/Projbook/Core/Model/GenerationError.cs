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
        /// The error line.
        /// </summary>
        public int Line { get; private set; }

        /// <summary>
        /// The error column.
        /// </summary>
        public int Column { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="ProjbookEngine"/>.
        /// </summary>
        /// <param name="sourceFile">Initializes the required <see cref="SourceFile"/>.</param>
        /// <param name="message">Initializes the required <see cref="Message"/>.</param>
        /// <param name="line">Initializes the required <see cref="Line"/>.</param>
        /// <param name="column">Initializes the required <see cref="Column"/>.</param>
        public GenerationError(string sourceFile, string message, int line, int column)
        {
            // Data validation
            Ensure.That(() => sourceFile).IsNotNullOrWhiteSpace();
            Ensure.That(() => message).IsNotNullOrWhiteSpace();

            // Initialize
            this.SourceFile = sourceFile;
            this.Message = message;
            this.Line = line;
            this.Column = column;
        }
    }
}