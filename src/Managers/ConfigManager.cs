using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Data.IRepositories;
using System.Text;
using SdtdServerKit.Data.Entities;

namespace SdtdServerKit.Managers
{
    /// <summary>
    /// 配置管理器
    /// </summary>
    public static class ConfigManager
    {
        private static GlobalSettings? _globalSettings;

        /// <summary>
        /// 配置改变事件
        /// </summary>
        public static event Action<ISettings>? SettingsChanged;

        /// <summary>
        /// 全局配置
        /// </summary>
        public static GlobalSettings GlobalSettings 
        {
            get 
            {
                if (_globalSettings == null)
                {
                    string language;
                    try
                    {
                        language = GamePrefs.GetString(EnumGamePrefs.Language);
                    }
                    catch
                    {
                        language = Language.English.ToString();
                        CustomLogger.Debug("从 GamePrefs 获取语言失败，已使用英语作为默认语言。");
                    }

                    _globalSettings = GetRequired<GlobalSettings>(Locales.Get(language), persist: false);
                }

                return _globalSettings;
            }
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <typeparam name="TSettings">配置类型</typeparam>
        /// <returns></returns>
        public static TSettings? Get<TSettings>() where TSettings : ISettings
        {
            TSettings? settings;

            string name = typeof(TSettings).Name;
            var settingsRepository = ModApi.ServiceContainer.Resolve<ISettingsRepository>();
            var entity = settingsRepository.GetById(name);
            if (entity == null)
            {
                return default;
            }
            else
            {
                settings = JsonConvert.DeserializeObject<TSettings>(entity.SerializedValue, ModApi.JsonSerializerSettings);
                if (settings == null)
                {
                    throw new InvalidOperationException($"The persistent settings: {name} is null!");
                }
            }

            return settings;
        }

        /// <summary>
        /// 获取配置, 如果不存在则加载默认配置
        /// </summary>
        /// <typeparam name="TSettings">配置类型</typeparam>
        /// <returns></returns>
        public static TSettings GetRequired<TSettings>(string locale, bool persist = true) where TSettings : ISettings
        {
            return Get<TSettings>() ?? LoadDefault<TSettings>(locale, persist);
        }

        /// <summary>
        /// 更新配置
        /// </summary>
        /// <typeparam name="TSettings">配置类型</typeparam>
        /// <param name="settings"></param>
        public static void Update<TSettings>(TSettings settings) where TSettings : ISettings
        {
            string name = typeof(TSettings).Name;

            try
            {
                if (settings is GlobalSettings globalSettings)
                {
                    _globalSettings = globalSettings;
                }

                SettingsChanged?.Invoke(settings);

                string json = JsonConvert.SerializeObject(settings, ModApi.JsonSerializerSettings);
                var settingsRepository = ModApi.ServiceContainer.Resolve<ISettingsRepository>();
                settingsRepository.InsertOrReplace(new T_Settings()
                {
                    Name = name,
                    SerializedValue = json
                });
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "Error in ConfigManager.Update, SettingsName: {name}", name);
            }
        }

        /// <summary>
        /// 加载默认配置
        /// </summary>
        /// <typeparam name="TSettings"></typeparam>
        /// <param name="locale"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        public static TSettings LoadDefault<TSettings>(string locale, bool persist = true) where TSettings : ISettings
        {
            try
            {
                string json = ModApi.GetDefaultConfigContent("functionsettings.json", locale);
                var jsonObject = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(json);

                if (jsonObject == null)
                {
                    throw new InvalidOperationException("Load settings faild, the json object is null.");
                }

                var settings = jsonObject[typeof(TSettings).Name]!.ToObject<TSettings>();
                if (settings == null)
                {
                    throw new InvalidOperationException("Load settings faild, the json can not deserialize.");
                }

                if (persist)
                {
                    Update(settings);
                }
                else if (settings is GlobalSettings globalSettings)
                {
                    _globalSettings = globalSettings;
                }

                return settings;
            }
            catch (Exception ex)
            {
                throw new Exception("Load default settings faild.", ex);
            }
        }
    }
}