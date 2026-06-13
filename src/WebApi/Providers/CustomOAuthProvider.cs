using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System.Security.Claims;

namespace SdtdServerKit.WebApi.Providers
{
    internal class CustomOAuthProvider : OAuthAuthorizationServerProvider
    {
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
            return Task.CompletedTask;
        }

        public override Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            if (CheckCredential(context.UserName, context.Password) == false)
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                return Task.CompletedTask;
            }

            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
            identity.AddClaim(new Claim("role", "user"));

            context.Validated(identity);

            return Task.CompletedTask;
        }

        private static bool CheckCredential(string userName, string password)
        {
            var appSettings = ModApi.AppSettings;
            //（账号或密码为空）时一律拒绝登录，引导用户先完成首次设置
            if (string.IsNullOrEmpty(appSettings.UserName) || string.IsNullOrEmpty(appSettings.Password))
            {
                return false;
            }
            return userName == appSettings.UserName && password == appSettings.Password;
        }

        public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            var newIdentity = new ClaimsIdentity(context.Ticket.Identity);

            var newTicket = new AuthenticationTicket(newIdentity, context.Ticket.Properties);
            context.Validated(newTicket);

            return Task.CompletedTask;
        }
    }
}