using HarmonyLib;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json.Serialization;
using Platform.Local;
using SdtdServerKit.Hooks;
using SdtdServerKit.Triggers;
using SdtdServerKit.WebApi;
using System.Text;
using WebSocketSharp.Server;
using MapRendering;
using Dapper;
using IceCoffee.SimpleCRUD.SqliteTypeHandlers;
using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data;
using System.Reflection;
using Autofac.Integration.WebApi;
using Microsoft.Data.Sqlite;
using SdtdServerKit.Managers;
using SdtdServerKit.WebSockets;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using SdtdServerKit.Constants;
using System.Runtime.InteropServices.ComTypes;
using System.Net;

namespace SdtdServerKit
{
    /// <summary>
    /// ModApi
    /// </summary>
    public class ModApi : IModApi
    {
        /// <summary>
        /// ModInstance
        /// </summary>
        public static Mod ModInstance { get; private set; } = null!;

        /// <summary>
        /// AppSettings
        /// </summary>
        public static AppSettings AppSettings { get; private set; } = null!;

        /// <summary>
        /// Main thread(the ui thread) synchronization context
        /// </summary>
        public static SynchronizationContext MainThreadSyncContext { get; private set; } = null!;

        /// <summary>
        /// Delegate used for executing commands.
        /// </summary>
        internal static ClientInfo CmdExecuteDelegate { get; private set; } = null!;

        /// <summary>
        /// Gets a value indicating whether the game has started.
        /// </summary>
        public static bool IsGameStartDone { get; private set; }

        /// <summary>
        /// Gets the JSON serializer settings.
        /// </summary>
        public static JsonSerializerSettings JsonSerializerSettings { get; private set; } = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            MissingMemberHandling = MissingMemberHandling.Ignore,
            TypeNameHandling = TypeNameHandling.None,
            DateFormatString = "yyyy-MM-dd HH:mm:ss"
        };

        /// <summary>
        /// InitMod
        /// </summary>
        /// <param name="modInstance"></param>
        public void InitMod(Mod modInstance)
        {
            try
            {
                ModInstance = modInstance;
                MainThreadSyncContext = SynchronizationContext.Current;
                CmdExecuteDelegate = new ClientInfo()
                {
                    PlatformId = new UserIdentifierLocal(modInstance.Name),
                };

                LoadAppSettings();

                LoadPlugins();

                PatchByHarmony();

                RegisterModEventHandlers();

            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "Initialize mod: " + modInstance.Name + " failed.");
            }
        }

        /// <summary>
        /// LoadedPlugins
        /// </summary>
        public static IReadOnlyList<Assembly> LoadedPlugins => _loadedPlugins;
        private static List<Assembly> _loadedPlugins = new List<Assembly>() { Assembly.GetExecutingAssembly() };
        private static void LoadPlugins()
        {
            string dir = Path.Combine(ModInstance.Path, "Plugins");

            if(Directory.Exists(dir) == false)
            {
                return;
            }

            string[] assemblyFiles = Directory.GetFiles(dir, "*.dll");
            foreach (var assemblyFile in assemblyFiles)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(assemblyFile);
                    _loadedPlugins.Add(assembly);

                    foreach (Type type in assembly.GetExportedTypes())
                    {
                        if (typeof(IModApi).IsAssignableFrom(type))
                        {
                            string assemblyName = Path.GetFileName(assembly.Location);
                            CustomLogger.Info("Found ModAPI in " + assemblyName + ", creating instance.");
                            var modApi = (IModApi)Activator.CreateInstance(type);
                            try
                            {
                                modApi.InitMod(ModInstance);
                                CustomLogger.Info("Initialized ModAPI instance on mod '" + ModInstance.Name + "' from DLL '" + assemblyName + "'");
                            }
                            catch (Exception ex)
                            {
                                CustomLogger.Error(ex, "Failed initializing ModAPI instance on mod '" + ModInstance.Name + "' from DLL '" + assemblyName + "'");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    CustomLogger.Error(ex, "Load plugin: " + assemblyFile + " failed.");
                }
            }
        }

        /// <summary>
        /// 获取默认配置文件路径
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="locale"></param>
        /// <returns></returns>
        internal static string GetDefaultConfigPath(string fileName, string? locale = null)
        {
            if(locale != null)
            {
                return Path.Combine(ModInstance.Path, "Config", "locales", locale, fileName);
            }
            else
            {
                return Path.Combine(ModInstance.Path, "Config", fileName);
            }
        }

        /// <summary>
        /// Get default config content
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="locale"></param>
        /// <returns></returns>
        internal static string GetDefaultConfigContent(string fileName, string locale)
        {
            string path = Path.Combine(ModInstance.Path, "Config", "locales", locale, fileName);
            if (File.Exists(path) == false)
            {
                CustomLogger.Warn("Load default settings faild, the file: {0} is not exists, use the default locale: {1}", path, Locales.EN);
                path = Path.Combine(ModInstance.Path, "Config", "locales", Locales.EN, fileName);
            }

            return File.ReadAllText(path, Encoding.UTF8);
        }

        private static void LoadAppSettings()
        {
            try
            {
                const string LSTY_Data = "LSTY_Data";
                string baseSettingsPath = Path.Combine(AppContext.BaseDirectory, LSTY_Data);
                
                string defaultAppConfigPath = Path.Combine(ModInstance.Path, "Config", "appsettings.json");
                string productionAppConfigPath = Path.Combine(baseSettingsPath, "appsettings.json");

                if (File.Exists(productionAppConfigPath) == false)
                {
                    try
                    {
                        Directory.CreateDirectory(baseSettingsPath);
                        File.Copy(defaultAppConfigPath, productionAppConfigPath);
                    }
                    catch (Exception ex)
                    {
                        CustomLogger.Warn(ex, $"Copy appsettings to production path {baseSettingsPath} faild, use the default path {defaultAppConfigPath}");
                    }
                }

                var builder = new ConfigurationBuilder()
                    .AddJsonFile(defaultAppConfigPath, optional: false, reloadOnChange: false)
                    .AddJsonFile(productionAppConfigPath, optional: true, reloadOnChange: false);

                var configuration = builder.Build();
                var appSettings = configuration.Get<AppSettings>();
                if (appSettings == null)
                {
                    throw new Exception("The app settings can not be null.");
                }
                else
                {
                    AppSettings = appSettings;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Load appsettings failed.", ex);
            }
        }

        private static WebSocketServer? _webSocketServer;
        private static void StartupOwinHost()
        {
            try
            {
                string webUrl = AppSettings.WebUrl;
                WebApp.Start<Startup>(webUrl);
                CustomLogger.Info("SdtdServerKit Web App Server running on " + webUrl);
            }
            catch (Exception ex)
            {
                throw new Exception("Startup Owin Host Server failed.", ex);
            }
        }
        private static void StartupWebSocket()
        {
            try
            {
                int webSocketPort = AppSettings.WebSocketPort;
                _webSocketServer = new WebSocketServer(webSocketPort);
                _webSocketServer.AddWebSocketService<WebSockets.Telnet>("/ws");
                _webSocketServer.Start();
                if (_webSocketServer.IsListening)
                {
                    var webSocketSessionManager = _webSocketServer.WebSocketServices["/ws"].Sessions;
                    Broadcaster.Init(webSocketSessionManager);
                    CustomLogger.Info("SdtdServerKit Web Socket Server listening on port " + webSocketPort);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Startup Web Socket failed.", ex);
            }
        }

        /// <summary>
        /// Gets the Harmony instance used for patching the mod.
        /// </summary>
        public static Harmony Harmony { get; private set; } = null!;
        /// <summary>
        /// Patch the mod using Harmony.
        /// </summary>
        private static void PatchByHarmony()
        {
            try
            {
                Harmony = new Harmony(ModInstance.Name);
                Harmony.PatchAll(typeof(ModApi).Assembly);

                CustomLogger.Info("Patch all by harmony success.");
            }
            catch (Exception ex)
            {
                throw new Exception("Patch by harmony failed.", ex);
            }
        }

        /// <summary>
        /// Get the map tile cache.
        /// </summary>
        /// <returns>The map tile cache.</returns>
        internal static MapTileCache? MapTileCache { private set; get; }
        private static void RegisterModEventHandlers()
        {
            try
            {
                Log.LogCallbacks += ModEventHub.OnLogCallback;
                ModEvents.GameAwake.RegisterHandler(ModEventHub.OnGameAwake);
                ModEvents.GameStartDone.RegisterHandler(ModEventHub.OnGameStartDone);
                ModEvents.GameShutdown.RegisterHandler(ModEventHub.OnGameShutdown);
                ModEvents.PlayerLogin.RegisterHandler(ModEventHub.OnPlayerLogin);
                ModEvents.PlayerSpawnedInWorld.RegisterHandler(ModEventHub.OnPlayerSpawnedInWorld);
                ModEvents.EntityKilled.RegisterHandler(ModEventHub.OnEntityKilled);
                ModEvents.PlayerDisconnected.RegisterHandler(ModEventHub.OnPlayerDisconnected);
                ModEvents.SavePlayerData.RegisterHandler(ModEventHub.OnSavePlayerData);
                ModEvents.ChatMessage.RegisterHandler(ModEventHub.OnChatMessage);
                ModEvents.PlayerSpawning.RegisterHandler(ModEventHub.OnPlayerSpawning);

                ModEventHub.GameStartDone += OnGameStartDone;

                CustomLogger.Info("Registered mod event handlers success.");
            }
            catch (Exception ex)
            {
                throw new Exception("Register mod event handlers failed.", ex);
            }
        }

        private static void OnGameStartDone()
        {
            try
            {
                WorldStaticDataHook.ReplaceXmls();
                GlobalTimer.RegisterSubTimer(new SubTimer(SkyChangeTrigger.Callback, 1) { IsEnabled = true });

                InitDependencyResolver();
                FunctionManager.Init();

                try
                {
                    string[] files = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "Mods"), "MapRendering.dll", SearchOption.AllDirectories);
                    if (files.Length == 0)
                    {
                        CustomLogger.Warn("It is detected that TFP Mod is not installed, some functions may not be available.");
                    }

                    MapTileCache = (MapTileCache)MapRenderer.GetTileCache();
                }
                catch (Exception ex)
                {
                    CustomLogger.Error(ex, "Load map tile cache failed, Please do not delete the default mod, You can verify the integrity of the game to solve this problem.");
                }

                StartupOwinHost();
                StartupWebSocket();

                IsGameStartDone = true;
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "Error in OnGameStartDone");
            }
        }

        /// <summary>
        /// DependencyResolver
        /// </summary>
        public static IContainer ServiceContainer { get; private set; } = null!;
        
        /// <summary>
        /// 只注册控制器、Function、数据库仓储相关类型
        /// </summary>
        private static void InitDependencyResolver()
        {
            var builder = new ContainerBuilder();

            #region 注册数据库仓储服务
            //if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            //{
            //    string destPath = Path.Combine(AppContext.BaseDirectory, "7DaysToDieServer_Data/MonoBleedingEdge/x86_64", "libdl.so");
            //    if (File.Exists(destPath) == false)
            //    {
            //        string srcPath = Path.Combine(ModInstance.Path, "libe_sqlite3.so");
            //        File.Copy(srcPath, destPath, true);
            //    }
            //}

            SqlMapper.AddTypeHandler(new GuidHandler());

            string databasePath = AppSettings.DatabasePath;
            if (Path.IsPathRooted(databasePath) == false)
            {
                databasePath = Path.Combine(AppContext.BaseDirectory, AppSettings.DatabasePath);
            }

            string connectionString = $"Data Source={databasePath};Cache=Shared";
            DbConnectionFactory.Default.ConfigureOptions(new DbConnectionOptions()
            {
                ConnectionString = connectionString,
                DbType = DbType.SQLite,
            });

            foreach (var item in _loadedPlugins)
            {
                builder.AddRepositories(item);
            }

            #endregion

            // Register Functions.
            foreach (var type in FunctionManager.GetFunctionTypes())
            {
                builder.RegisterType(type).SingleInstance();
            }

            // Register your Web API controllers.
            builder.RegisterApiControllers(_loadedPlugins.ToArray());

            // Run other optional steps, like registering filters,
            // per-controller-type services, etc., then set the dependency resolver
            // to be Autofac.
            var container = builder.Build();

            ServiceContainer = container;

            InitDatabase();
        }

        /// <summary>
        /// 初始化数据库
        /// </summary>
        private static void InitDatabase()
        {
            try
            {
                if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_winsqlite3());
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_sqlite3());
                }

                var dbConnection = DbConnectionFactory.Default.CreateConnection(DbAliases.Default);

                string dataSource = ((SqliteConnection)dbConnection).DataSource;
                var fileInfo = new FileInfo(dataSource);
                if (fileInfo.Exists == false)
                {
                    if (fileInfo.DirectoryName != null)
                    {
                        Directory.CreateDirectory(fileInfo.DirectoryName);
                    }
                    File.Create(dataSource).Close();
                }

                string path = Path.Combine(ModInstance.Path, "Config/sql");
                var di = new DirectoryInfo(path);
                var files = di.GetFiles("*.sql");

                foreach (var file in files)
                {
                    string sql = File.ReadAllText(file.FullName, Encoding.UTF8);
                    dbConnection.Execute(sql);
                }
                TryAddColumn(dbConnection, "T_VipGift_v1", "PlayerName", "TEXT");
                CustomLogger.Info("Initialize database success.");
            }
            catch (Exception ex)
            {
                throw new Exception("Initialize database error.", ex);
            }
        }

        /// <summary>
        /// 安全添加数据库列
        /// </summary>
        private static void TryAddColumn(System.Data.IDbConnection dbConnection, string tableName, string columnName, string columnType)
        {
            try
            {
                dbConnection.Execute($"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnType}");
            }
            catch
            {
            }
        }
    }
}
