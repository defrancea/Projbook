using Newtonsoft.Json;

namespace Projbook.Core.Model.Configuration
{
    /// <summary>
    /// Represents a document configuration.
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// The document title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The template file for html generation.
        /// </summary>
        [JsonProperty("template-html")]
        public string TemplateHtml { get; set; }

        /// <summary>
        /// The template file for pdf generation.
        /// </summary>
        [JsonProperty("template-pdf")]
        public string TemplatePdf { get; set; }

        /// <summary>
        /// The output file for html generation.
        /// </summary>
        [JsonProperty("output-html")]
        public string OutputHtml { get; set; }

        /// <summary>
        /// The output file for pdf generation.
        /// </summary>
        [JsonProperty("output-pdf")]
        public string OutputPdf { get; set; }

        /// <summary>
        /// The section title base.
        /// </summary>
        [JsonProperty("section-title-base")]
        public int SectionTitleBase { get; set; }

        /// <summary>
        /// Configuration pages.
        /// </summary>
        public Page[] Pages { get; set; }

        /// <summary>
        /// Whether or not the html has to be generated.
        /// </summary>
        public bool GenerateHtml
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.TemplateHtml) && !string.IsNullOrWhiteSpace(this.OutputHtml);
            }
        }

        /// <summary>
        /// Whether or not the pdf has to be generated.
        /// </summary>
        public bool GeneratePdf
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.TemplatePdf) && !string.IsNullOrWhiteSpace(this.OutputPdf);
            }
        }
    }
}