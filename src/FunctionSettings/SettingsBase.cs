namespace SdtdServerKit.FunctionSettings
{
    /// <summary>
    /// Settings Base
    /// </summary>
    public abstract class SettingsBase : ISettings
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual bool IsEnabled { get; set; }
    }
}
