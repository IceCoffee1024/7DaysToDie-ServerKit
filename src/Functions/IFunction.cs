namespace SdtdServerKit.Functions
{
    /// <summary>
    /// Function Interface
    /// </summary>
    public interface IFunction
    {
        /// <summary>
        /// Whether the function is running
        /// </summary>
        public bool IsRunning { get; }

        /// <summary>
        /// Function Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Load Settings
        /// </summary>
        internal void LoadSettings();
    }
}