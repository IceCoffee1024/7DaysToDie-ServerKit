using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security.DataHandler;
using SdtdServerKit.WebApi.DataProtection;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace SdtdServerKit.WebSockets
{
    internal class Telnet : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            if(string.Equals(e.Data, "exit", StringComparison.OrdinalIgnoreCase))
            {
                Close();
                return;
            }

            var executeResult = Enumerable.Empty<string>();
            ModApi.MainThreadSyncContext.Send((command) =>
            {
                executeResult = SdtdConsole.Instance.ExecuteSync((string)command, ModApi.CmdExecuteDelegate);
            }, e.Data);

            var message = new WebSocketMessage<IEnumerable<string>>(ModEventType.CommandExecutionReply, executeResult);
            string json = JsonConvert.SerializeObject(message, ModApi.JsonSerializerSettings);
            Send(json);
        }

        protected override void OnOpen()
        {
            string token = base.QueryString["token"] ?? base.Headers["token"];
            var ticket = new TicketDataFormat(CustomDataProtectionProvider.DataProtector).Unprotect(token);

            if (ticket == null)
            {
                Close(CloseStatusCode.InvalidData, "Invalid bearer token received.");
                return;
            }

            // Validate expiration time if present
            DateTimeOffset currentUtc = new SystemClock().UtcNow;
            if (ticket.Properties.ExpiresUtc.HasValue && ticket.Properties.ExpiresUtc.Value < currentUtc)
            {
                Close(CloseStatusCode.InvalidData, "Expired bearer token received.");
                return;
            }

            Welcome();
            CustomLogger.Debug("WebSocket connection established. Remote endpoint " + base.UserEndPoint.ToString());
        }

        private void Welcome()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("*** Connected with 7DTD server.");
            stringBuilder.AppendLine("*** Server version: " + global::Constants.cVersionInformation.LongString + " Compatibility Version: " + global::Constants.cVersionInformation.LongStringNoBuild);
            stringBuilder.AppendLine("*** Dedicated server only build");
            stringBuilder.AppendLine(string.Empty);
            stringBuilder.AppendLine("Server IP:   " + (string.IsNullOrEmpty(GamePrefs.GetString(EnumGamePrefs.ServerIP)) ? "Any" : GamePrefs.GetString(EnumGamePrefs.ServerIP)));
            stringBuilder.AppendLine("Server port: " + GamePrefs.GetInt(EnumGamePrefs.ServerPort).ToString());
            stringBuilder.AppendLine("Max players: " + GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount).ToString());
            stringBuilder.AppendLine("Game mode:   " + GamePrefs.GetString(EnumGamePrefs.GameMode));
            stringBuilder.AppendLine("World:       " + GamePrefs.GetString(EnumGamePrefs.GameWorld));
            stringBuilder.AppendLine("Game name:   " + GamePrefs.GetString(EnumGamePrefs.GameName));
            stringBuilder.AppendLine("Difficulty:  " + GamePrefs.GetInt(EnumGamePrefs.GameDifficulty).ToString());
            stringBuilder.AppendLine(string.Empty);
            stringBuilder.AppendLine("Press 'help' to get a list of all commands.");
            stringBuilder.AppendLine(string.Empty);

            string json = JsonConvert.SerializeObject(new WebSocketMessage<string>(ModEventType.Welcome, stringBuilder.ToString()), ModApi.JsonSerializerSettings);
            Send(json);
        }
    }
}
