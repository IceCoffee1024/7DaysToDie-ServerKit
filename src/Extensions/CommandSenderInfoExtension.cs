namespace SdtdServerKit.Extensions
{
    /// <summary>
    /// Extension methods for CommandSenderInfo.
    /// </summary>
    public static class CommandSenderInfoExtension
    {
        /// <summary>
        /// Get the EntityId of the sender.
        /// </summary>
        /// <param name="senderInfo"></param>
        /// <returns></returns>
        public static int GetEntityId(this CommandSenderInfo senderInfo)
        {
            var remoteClientInfo = senderInfo.RemoteClientInfo;
            return remoteClientInfo == null ? -1 : remoteClientInfo.entityId;
        }
    }
}
