using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.OAuth;
using SdtdServerKit.WebApi.DataProtection;
using SdtdServerKit.WebApi.Providers;
using System.Net;
using System.Security.Claims;

namespace SdtdServerKit.WebApi.Middlewares
{
    /// <summary>
    ///
    /// </summary>
    public class SteamReturnMiddleware : OwinMiddleware
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="next"></param>
        public SteamReturnMiddleware(OwinMiddleware next) : base(next)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task Invoke(IOwinContext context)
        {
            // 检查请求路径是否为 /api/auth/steam/return
            if (context.Request.Path == new PathString("/api/auth/steam/return"))
            {
                try
                {
                    // 处理 Steam 回调逻辑
                    var queryParams = context.Request.Query;

                    // 1. 获取 Steam 返回的参数（例如 `openid.claimed_id` 等）
                    string claimedId = queryParams["openid.claimed_id"];
                    string openidMode = queryParams["openid.mode"];
                    string redirect = queryParams["redirect"];

                    // 2. 校验参数是否符合预期（这里可以加入更多的验证逻辑）
                    if (string.IsNullOrEmpty(claimedId) || openidMode != "id_res")
                    {
                        string message = "Invalid Steam OAuth callback.";
                        string errorRedirectUrl = "/#/error?message=" + message;
                        context.Response.Redirect(errorRedirectUrl);
                        return;
                    }

                    // 3. 验证 Steam 登录是否有效
                    bool isValid = VerifySteamLogin(context.Request.QueryString.ToString());
                    if (isValid == false)
                    {
                        string message = "Steam login verification failed.";
                        string errorRedirectUrl = "/#/error?message=" + message;
                        context.Response.Redirect(errorRedirectUrl);
                        return;
                    }

                    // 4. 提取 Steam ID
                    string playerName = string.Empty;
                    string steamId = ExtractSteamId(claimedId);
                    var eos = GetEOS(steamId);
                    if (eos == null)
                    {
                        string message = "You are not a registered player.";
                        string errorRedirectUrl = "/#/403?message=" + message;
                        context.Response.Redirect(errorRedirectUrl);
                        return;
                    }

                    if (GameManager.Instance.persistentPlayers.Players.TryGetValue(eos, out var player))
                    {
                        playerName = player.PlayerName.DisplayName;
                    }

                    bool isAdmin = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(eos) == 0;
                    if (isAdmin == false)
                    {
                        string message = "You are not a level 0 administrator.";
                        string errorRedirectUrl = "/#/403?message=" + message;
                        context.Response.Redirect(errorRedirectUrl);
                        return;
                    }

                    // 5. 生成 JWT Token 或其他必要信息
                    string accessToken = GenerateJwtToken(steamId);
                    string refreshToken = Guid.NewGuid().ToString("n");
                    CustomRefreshTokenProvider.AddToken(refreshToken, accessToken);

                    // 6. 重定向到前端页面
                    string redirectUrl = $"/#/loginSuccess?steamid={steamId}&playerName={playerName}&accessToken={accessToken}&expiresIn={ModApi.AppSettings.AccessTokenExpireTime}&refreshToken={refreshToken}&redirect={redirect}";
                    context.Response.Redirect(redirectUrl);

                    return; // 确保不调用下一个中间件
                }
                catch (Exception ex)
                {
                    string message = $"Error: {ex.Message}";
                    string errorRedirectUrl = "/#/error?message=" + message;
                    context.Response.Redirect(errorRedirectUrl);
                    CustomLogger.Warn(ex.ToString());
                    return;
                }
            }

            // 如果不是 /api/auth/steam/return，调用下一个中间件
            await Next.Invoke(context);
        }

        private static string ExtractSteamId(string claimedId)
        {
            if (string.IsNullOrEmpty(claimedId))
            {
                throw new ArgumentException("Claimed ID cannot be null or empty.");
            }

            // 检查 URL 是否以 Steam 的 OpenID 前缀开头
            const string steamOpenIdPrefix = "https://steamcommunity.com/openid/id/";

            if (claimedId.StartsWith(steamOpenIdPrefix, StringComparison.OrdinalIgnoreCase) == false)
            {
                throw new ArgumentException("Invalid Steam Claimed ID format.");
            }

            // 提取 Steam ID
            return claimedId.Substring(steamOpenIdPrefix.Length);
        }

        private static string GenerateJwtToken(string userName)
        {
            var identity = new ClaimsIdentity(OAuthDefaults.AuthenticationType);
            identity.AddClaim(new Claim(ClaimTypes.Name, userName));
            identity.AddClaim(new Claim("role", "user"));
            var newIdentity = new ClaimsIdentity(identity);
            DateTime utcNow = DateTime.UtcNow;
            var props = new AuthenticationProperties()
            {
                IssuedUtc = utcNow,
                ExpiresUtc = utcNow.AddSeconds(ModApi.AppSettings.AccessTokenExpireTime),
            };
            var newTicket = new AuthenticationTicket(newIdentity, props);
            return new TicketDataFormat(CustomDataProtectionProvider.DataProtector).Protect(newTicket);
        }

        private static PlatformUserIdentifierAbs? GetEOS(string steamId)
        {
            foreach (var item in GameManager.Instance.persistentPlayers.Players.Values)
            {
                if (item.NativeId.ReadablePlatformUserIdentifier == steamId)
                {
                    return item.PrimaryId;
                }
            }

            return null;
        }

        private static bool VerifySteamLogin(string queryString)
        {
            //queryString = Regex.Replace(queryString, "(?<=openid.mode=).+?(?=\\&)", "check_authentication", RegexOptions.IgnoreCase);

            int startIndex = queryString.IndexOf("openid.mode=", StringComparison.OrdinalIgnoreCase);
            if (startIndex != -1)
            {
                startIndex += "openid.mode=".Length;
                int endIndex = queryString.IndexOf("&", startIndex);
                if (endIndex == -1) endIndex = queryString.Length;

                // 替换 openid.mode 的值为 "check_authentication"
                queryString = queryString.Substring(0, startIndex) + "check_authentication" + queryString.Substring(endIndex);
            }

            // Steam OpenID 验证地址
            string steamOpenIdUrl = "https://steamcommunity.com/openid/login" + queryString;

            using (WebClient webClient = new WebClient())
            {
                // webClient.Proxy = new WebProxy("http://127.0.0.1:10809");
                string str = webClient.DownloadString(steamOpenIdUrl);
                return str.Contains("is_valid:true");
            }
        }
    }
}