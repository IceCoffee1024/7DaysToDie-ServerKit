using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin;
using Owin;
using NSwag.AspNet.Owin;
using NSwag.Generation.Processors.Security;
using NSwag;
using SdtdServerKit.WebApi.Providers;
using NJsonSchema;
using Microsoft.Owin.Security.DataProtection;
using System.Net.Http.Formatting;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.FileSystems;
using NJsonSchema.NewtonsoftJson.Generation;
using Autofac.Integration.WebApi;
using SdtdServerKit.WebApi.DataProtection;
using SdtdServerKit.WebApi.Middlewares;
using System.Web.Http.Dispatcher;

namespace SdtdServerKit.WebApi
{
    /// <summary>
    /// The Startup class is specified as a type parameter in the WebApp.Start method.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// OAuth token endpoint path
        /// </summary>
        public const string OAuthTokenEndpointPath = "/api/oauth/token";

        /// <summary>
        /// OAuth steam return path
        /// </summary>
        public const string OAuthSteamReturnPath = "/api/oauth/steam/return";

        /// <summary>
        /// This code configures Web API.
        /// </summary>
        /// <param name="app"></param>
        public void Configuration(IAppBuilder app)
        {
            // Configure Web API for self-host.
            var config = new HttpConfiguration()
            {
                IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always,
                DependencyResolver = new AutofacWebApiDependencyResolver(ModApi.ServiceContainer)
            };
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            //config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.Formatters.Add(new BsonMediaTypeFormatter());
            config.Formatters.JsonFormatter.SerializerSettings = ModApi.JsonSerializerSettings;
            //config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html")); // Default return json
            config.Filters.Add(new ValidateModelAttribute());

            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    CustomLogger.Error(ex, "Error in Owin Host GlobalExceptionHandleMiddleware.");
                }
            });

            app.Use<SteamReturnMiddleware>();

            string webRootPath = Path.Combine(ModApi.ModInstance.Path, "wwwroot");
            if (Directory.Exists(webRootPath))
            {
                var fileSystem = new PhysicalFileSystem(webRootPath);
                // Serve the default file, if present.
                app.UseDefaultFiles(new DefaultFilesOptions()
                {
                    DefaultFileNames = new string[] { "index.html" },
                    FileSystem = fileSystem,
                    RequestPath = PathString.Empty
                });
                // Serve static files.
                app.UseStaticFiles(new StaticFileOptions()
                {
                    FileSystem = fileSystem,
                    RequestPath = PathString.Empty
                });
            }

            //app.UseSwaggerUi(ModApi.LoadedPlugins, settings =>
            //{
            //    // configure settings here
            //    // settings.GeneratorSettings.*: Generator settings and extension points
            //    // settings.*: Routing and UI settings

            //    // 可以设置从注释文件加载, 但是加载的内容可被 OpenApiTagAttribute 特性覆盖
            //    settings.GeneratorSettings.UseControllerSummaryAsTagDescription = true;
            //    settings.GeneratorSettings.OperationProcessors.Add(new SdtdServerKit.WebApi.OperationSecurityScopeProcessor("JWT Token"));
            //    settings.GeneratorSettings.DocumentProcessors.Add(new SecurityDefinitionAppender("JWT Token",
            //        new OpenApiSecurityScheme()
            //        {
            //            Type = OpenApiSecuritySchemeType.ApiKey,
            //            Name = "Authorization",
            //            Description = "Copy 'Bearer ' + valid JWT token into field",
            //            In = OpenApiSecurityApiKeyLocation.Header,
            //        }));
            //    settings.GeneratorSettings.ApplySettings(new NewtonsoftJsonSchemaGeneratorSettings { SerializerSettings = ModApi.JsonSerializerSettings, SchemaType = SchemaType.OpenApi3 }, null);
            //    settings.PostProcess = (document) =>
            //    {
            //        document.Info.Version = "v1";
            //        document.Info.Title = "7DaysToDie-ServerKit RESTful APIs Documentation";
            //        document.Info.Description = "RESTful APIs Documentation for 7 Days to Die dedicated servers.";
            //        document.Info.TermsOfService = "https://7dtd.top";
            //        document.Info.Contact = new OpenApiContact()
            //        {
            //            Name = "LuoShuiTianTi",
            //            Email = "IceCoffee1024@qq.com",
            //            Url = "https://github.com/IceCoffee1024"
            //        };
            //        document.Info.License = new OpenApiLicense()
            //        {
            //            Name = "LICENSE",
            //            Url = "https://github.com/IceCoffee1024/7DaysToDie-ServerKit/blob/main/README.md"
            //        };

            //        AddOAuthTokenEndpointApiSchema(document);
            //    };
            //});

            app.SetDataProtectionProvider(new CustomDataProtectionProvider());
            // Token Generation
            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString(OAuthTokenEndpointPath),
                AccessTokenExpireTimeSpan = TimeSpan.FromSeconds(ModApi.AppSettings.AccessTokenExpireTime),
                Provider = new CustomOAuthProvider(),
                RefreshTokenProvider = new CustomRefreshTokenProvider()
            });

            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

            app.Use(async (context, next) =>
            {
                if (ModApi.IsGameStartDone == false)
                {
                    var error = new InternalServerError() { Message = "The game is still initializing." };
                    string json = JsonConvert.SerializeObject(error, ModApi.JsonSerializerSettings);
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                    await context.Response.WriteAsync(json);
                }
                else
                {
                    await next();
                }
            });

            // Register the Autofac middleware FIRST, then the Autofac Web API middleware,
            // and finally the standard Web API middleware.
            // 注册 Autofac 中间件, 支持请求范围和中间件注入
            app.UseAutofacMiddleware(ModApi.ServiceContainer);

            // 将 Autofac 容器与 Web API 集成, 设置 Web API 的依赖解析器为 Autofac, 并确保 Web API 控制器及其依赖项由 Autofac 容器管理
            app.UseAutofacWebApi(config);

            // 将 Web API 的运行时（中间件）添加到 OWIN 管道中, 负责处理 HTTP 请求并将它们路由到正确的 Web API 控制器和操作方法
            app.UseWebApi(config);

            config.Services.Replace(typeof(IHttpControllerTypeResolver), new CustomHttpControllerTypeResolver());

            config.EnsureInitialized();
        }

        private static void AddOAuthTokenEndpointApiSchema(OpenApiDocument document)
        {
            var tokenOpr = new OpenApiOperation()
            {
                OperationId = "OAuth_Token",
                Consumes = new List<string>() { "application/x-www-form-urlencoded" },
                Produces = new List<string>() { "application/json" },
                Tags = new List<string>() { "Authentication" },
                Summary = "User login with form data",
                Description = "Get the access token used for webapp.",
                RequestBody = new OpenApiRequestBody()
                {
                    Description = "User login with form data",
                    IsRequired = true,
                }
            };

            tokenOpr.RequestBody.Content["application/x-www-form-urlencoded"] = new OpenApiMediaType()
            {
                Schema = new JsonSchema()
                {
                    Type = JsonObjectType.Object,
                    Properties =
                    {
                        ["grant_type"] = new JsonSchemaProperty { Type = JsonObjectType.String, IsRequired = true, Default = "password" },
                        ["username"] = new JsonSchemaProperty { Type = JsonObjectType.String, IsRequired = false },
                        ["password"] = new JsonSchemaProperty { Type = JsonObjectType.String, IsRequired = false },
                        ["refresh_token"] = new JsonSchemaProperty { Type = JsonObjectType.String, IsRequired = false }
                    }
                }
            };

            //tokenOpr.Parameters.Add(new OpenApiParameter()
            //{
            //    Name = "grant_type",
            //    Description = "",
            //    IsRequired = true,
            //    Kind = OpenApiParameterKind.FormData,
            //    Type = JsonObjectType.String,
            //    Default = "password"
            //});
            //tokenOpr.Parameters.Add(new OpenApiParameter()
            //{
            //    Name = "username",
            //    Description = "",
            //    IsRequired = false,
            //    Kind = OpenApiParameterKind.FormData,
            //    Type = JsonObjectType.String
            //});
            //tokenOpr.Parameters.Add(new OpenApiParameter()
            //{
            //    Name = "password",
            //    Description = "",
            //    IsRequired = false,
            //    Kind = OpenApiParameterKind.FormData,
            //    Type = JsonObjectType.String
            //});
            //tokenOpr.Parameters.Add(new OpenApiParameter()
            //{
            //    Name = "refresh_token",
            //    Description = "",
            //    IsRequired = false,
            //    Kind = OpenApiParameterKind.FormData,
            //    Type = JsonObjectType.String
            //});

            var resp200 = new OpenApiResponse()
            {
                Description = "Authentication token and meta data.",
                Schema = new JsonSchema()
            };
            resp200.Schema.Properties.Add("access_token", new JsonSchemaProperty { Type = JsonObjectType.String, IsRequired = true });
            resp200.Schema.Properties.Add("token_type", new JsonSchemaProperty { Type = JsonObjectType.String, IsRequired = true });
            resp200.Schema.Properties.Add("expires_in", new JsonSchemaProperty { Type = JsonObjectType.Integer, IsRequired = true, Format = JsonFormatStrings.Integer });
            resp200.Schema.Properties.Add("refresh_token", new JsonSchemaProperty { Type = JsonObjectType.String, IsRequired = false });

            var resp400 = new OpenApiResponse()
            {
                Description = "Authentication error.",
                Schema = new JsonSchema()
            };
            resp400.Schema.Properties.Add("error", new JsonSchemaProperty { Type = JsonObjectType.String, IsRequired = true });
            resp400.Schema.Properties.Add("error_description", new JsonSchemaProperty { Type = JsonObjectType.String, IsRequired = true });

            tokenOpr.Responses.Add("200", resp200);
            tokenOpr.Responses.Add("400", resp400);

            var path = new OpenApiPathItem();
            path.Add(OpenApiOperationMethod.Post, tokenOpr);
            document.Paths.Add(OAuthTokenEndpointPath, path);
        }
    }
}
