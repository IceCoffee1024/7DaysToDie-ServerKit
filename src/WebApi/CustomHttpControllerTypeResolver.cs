using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace SdtdServerKit.WebApi
{
    internal class CustomHttpControllerTypeResolver : DefaultHttpControllerTypeResolver
    {
        public CustomHttpControllerTypeResolver() : base(IsControllerType)
        {
        }

        public static bool IsControllerType(Type t)
        {
            try
            {
                return
                t != null &&
                t.IsClass &&
                t.IsVisible &&
                !t.IsAbstract &&
                t.Assembly == Assembly.GetExecutingAssembly() &&
                typeof(IHttpController).IsAssignableFrom(t);
            }
            catch
            {
                return false;
            }
        }
    }
}
