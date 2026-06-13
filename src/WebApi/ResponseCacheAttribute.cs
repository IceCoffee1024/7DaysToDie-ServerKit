using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace SdtdServerKit.WebApi
{
    public class ResponseCacheAttribute : System.Web.Http.Filters.ActionFilterAttribute
    {
        public int Duration { get; set; }

        public ResponseCacheAttribute()
        {
            Duration = 3600;
        }

        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            if (context.Response != null && context.Response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                context.Response.Headers.CacheControl = new CacheControlHeaderValue()
                {
                    Public = true,
                    MaxAge = TimeSpan.FromSeconds(Duration)
                };
            }

            base.OnActionExecuted(context);
        }
    }
}
