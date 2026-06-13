namespace SdtdServerKit.Models
{
    /// <summary>
    /// Represents the event when the sky changes.
    /// </summary>
    public class SkyChanged
    {
        /// <summary>
        /// Gets or sets the type of sky change event.
        /// </summary>
        public SkyChangeEventType SkyChangeEventType { get; set; }

        /// <summary>
        /// Gets or sets the hour of dawn.
        /// </summary>
        public int DawnHour { get; set; }

        /// <summary>
        /// Gets or sets the hour of dusk.
        /// </summary>
        public int DuskHour { get; set; }

        /// <summary>
        /// Gets or sets the remaining days until the blood moon.
        /// </summary>
        public int BloodMoonDaysRemaining { get; set; }
    }
}