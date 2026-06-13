namespace SdtdServerKit.FunctionSettings
{
    public class TeleportHomeSettings : SettingsBase
    {
        /// <summary>
        /// Command to query the Home list
        /// </summary>
        public required string QueryListCmd { get; set; }

        /// <summary>
        /// Teleport interval in seconds
        /// </summary>
        public int TeleInterval { get; set; }

        /// <summary>
        /// Prefix for the set Home command
        /// </summary>
        public required string SetHomeCmdPrefix { get; set; }

        /// <summary>
        /// Maximum number of Homes that can be set
        /// </summary>
        public int SetCountLimit { get; set; }

        /// <summary>
        /// Points required to set a Home
        /// </summary>
        public int PointsRequiredForSet { get; set; }

        /// <summary>
        /// Prefix for the delete Home command
        /// </summary>
        public required string DeleteHomeCmdPrefix { get; set; }

        /// <summary>
        /// Prefix for the teleport Home command
        /// </summary>
        public required string TeleHomeCmdPrefix { get; set; }

        /// <summary>
        /// Points required to teleport to a Home
        /// </summary>
        public int PointsRequiredForTele { get; set; }

        /// <summary>
        /// Tip when no Home is found
        /// </summary>
        public required string NoHomeTip { get; set; }

        /// <summary>
        /// Tip for querying the list
        /// </summary>
        public required string LocationItemTip { get; set; }

        /// <summary>
        /// Tip when the limit is exceeded
        /// </summary>
        public required string OverLimitTip { get; set; }

        /// <summary>
        /// Tip when there are not enough points to set a Home
        /// </summary>
        public required string SetPointsNotEnoughTip { get; set; }

        /// <summary>
        /// Tip when setting a Home is successful
        /// </summary>
        public required string SetSuccessTip { get; set; }

        /// <summary>
        /// Tip when overwriting a Home is successful
        /// </summary>
        public required string OverwriteSuccessTip { get; set; }

        /// <summary>
        /// Tip when deleting a Home is successful
        /// </summary>
        public required string DeleteSuccessTip { get; set; }

        /// <summary>
        /// Tip when the Home is not found
        /// </summary>
        public required string HomeNotFoundTip { get; set; }

        /// <summary>
        /// Tip when the teleport is cooling down
        /// </summary>
        public required string CoolingTip { get; set; }

        /// <summary>
        /// Tip when there are not enough points to teleport
        /// </summary>
        public required string TelePointsNotEnoughTip { get; set; }

        /// <summary>
        /// Tip when teleporting is successful
        /// </summary>
        public required string TeleSuccessTip { get; set; }
    }
}