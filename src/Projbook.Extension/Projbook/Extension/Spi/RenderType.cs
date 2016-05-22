namespace Projbook.Extension.Spi
{
    /// <summary>
    /// Defines how a snippet should be rendered.
    /// </summary>
    public enum RenderType
    {
        /// <summary>
        /// Inject snippet content.
        /// </summary>
        Inject = 0,

        /// <summary>
        /// Override placeholder with the snippet content.
        /// </summary>
        Override = 1
    }
}
