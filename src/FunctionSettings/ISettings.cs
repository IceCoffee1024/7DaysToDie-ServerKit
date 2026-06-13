namespace SdtdServerKit.FunctionSettings
{
    /// <summary>
    /// Settings Interface
    /// </summary>
    public interface ISettings
    {
        /// <summary>
        /// Whether to enable the function
        /// </summary>
        public bool IsEnabled { get; set; }
    }
}