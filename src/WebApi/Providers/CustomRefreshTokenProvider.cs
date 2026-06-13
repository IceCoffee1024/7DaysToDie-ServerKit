using ConcurrentCollections;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Linq;
using SdtdServerKit.WebApi.DataProtection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SdtdServerKit.WebApi.Providers
{
    internal class CustomRefreshTokenProvider : AuthenticationTokenProvider
    {
        private static readonly ConcurrentDictionary<string, (string, DateTime)> _refreshTokenCache = new();

        internal static void AddToken(string refreshToken, string protectedTicket)
        {
            _refreshTokenCache[refreshToken] = (protectedTicket, DateTime.UtcNow);
        }

        // 生成 Refresh Token
        public override void Create(AuthenticationTokenCreateContext context)
        {
            var refreshToken = Guid.NewGuid().ToString("n");

            DateTime utcNow = DateTime.UtcNow;
            context.Ticket.Properties.IssuedUtc = utcNow;
            context.Ticket.Properties.ExpiresUtc = utcNow.AddDays(7);

            _refreshTokenCache[refreshToken] = (context.SerializeTicket(), utcNow);

            context.SetToken(refreshToken);
            //Task.Run(() =>
            //{
            //    // 清理过期的 Refresh Token
            //    var expiredTokens = _refreshTokenCache.Where(x => x.Value.Item2 < DateTime.UtcNow.AddDays(-7)).Select(x => x.Key).ToList();
            //    foreach (var expiredToken in expiredTokens)
            //    {
            //        _refreshTokenCache.TryRemove(expiredToken, out _);
            //    }
            //});
        }

        // 验证 Refresh Token
        public override void Receive(AuthenticationTokenReceiveContext context)
        {
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            // 验证 Refresh Token 的逻辑（从数据库或缓存中检索 Token）
            var refreshToken = context.Token;

            // 如果验证通过，则重新生成 Access Token
            if (string.IsNullOrEmpty(refreshToken) == false && _refreshTokenCache.TryRemove(refreshToken, out var item))
            {
                context.DeserializeTicket(item.Item1);
            }
        }
    }

}
