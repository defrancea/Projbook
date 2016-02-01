using Projbook.Core.Model;

namespace Projbook.Core.Exception
{
    /// <summary>
    /// Represents a configuration loading exception.
    /// </summary>
    public class ConfigurationException : System.Exception
    {
        /// <summary>
        /// The generation errors.
        /// </summary>
        public GenerationError[] GenerationErrors { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="ConfigurationException"/>.
        /// </summary>
        /// <param name="generationErrors">THe generation errors.</param>
        public ConfigurationException(params GenerationError[] generationErrors)
            : base ("Error occured during configuration loading")
        {
            // Initialize
            this.GenerationErrors = generationErrors;
        }
    }
}