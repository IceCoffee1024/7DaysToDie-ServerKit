namespace SdtdServerKit.FunctionSettings
{
    /// <summary>
    /// Teleport Friend Settings
    /// </summary>
    public class TeleportFriendSettings : SettingsBase
    {
        /// <summary>
        /// Teleport command prefix
        /// </summary>
        public string TeleCmdPrefix { get; set; }

        /// <summary>
        /// Teleport interval, in seconds
        /// </summary>
        public int TeleInterval { get; set; }

        /// <summary>
        /// Points required
        /// </summary>
        public int PointsRequired { get; set; }

        /// <summary>
        /// Teleport success tip
        /// </summary>
        public string TeleSuccessTip { get; set; }

        /// <summary>
        /// Points not enough tip
        /// </summary>
        public string PointsNotEnoughTip { get; set; }

        /// <summary>
        /// Cooling tip
        /// </summary>
        public string CoolingTip { get; set; }

        /// <summary>
        /// Target not found tip
        /// </summary>
        public string TargetNotFoundTip { get; set; }

        /// <summary>
        /// Whether to bypass the friend check
        /// </summary>
        public bool IsFriendBypass { get; set; }

        /// <summary>
        /// Teleport confirm tip
        /// </summary>
        public string TeleConfirmTip { get; set; }

        /// <summary>
        /// Teleport accept
        /// </summary>
        public string AcceptTele { get; set; }

        /// <summary>
        /// Teleport reject
        /// </summary>
        public string RejectTele { get; set; }

        /// <summary>
        /// Target teleport reject tip
        /// </summary>
        public string TargetRejectTeleTip { get; set; }

        /// <summary>
        /// Keep duration
        /// </summary>
        public int KeepDuration { get; set; }
    }
}