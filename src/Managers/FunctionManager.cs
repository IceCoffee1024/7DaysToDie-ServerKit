using Autofac;
using SdtdServerKit.Functions;
using System.Reflection;

namespace SdtdServerKit.Managers
{
    /// <summary>
    /// 功能管理器
    /// </summary>
    public static class FunctionManager
    {
        private static readonly Dictionary<string, IFunction> _functionDict = new Dictionary<string, IFunction>();

        /// <summary>
        /// 加载所有功能
        /// </summary>
        public static void Init()
        {
            try
            {
                foreach (var type in GetFunctionTypes())
                {
                    var function = (IFunction)ModApi.ServiceContainer.Resolve(type);
                    function.LoadSettings();
                    _functionDict.Add(function.Name, function);
                }

                CustomLogger.Debug($"Loaded {_functionDict.Count} functions.");
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "Error in FunctionManager.LoadFunctions");
            }
        }

        internal static List<Type> GetFunctionTypes()
        {
            var types = new List<Type>();

            foreach (var assembly in ModApi.LoadedPlugins)
            {
                foreach (var type in assembly.GetExportedTypes())
                {
                    if (typeof(IFunction).IsAssignableFrom(type) && type.IsAbstract == false)
                    {
                        types.Add(type);
                    }
                }
            }
            
            return types;
        }

        /// <summary>
        /// 获取所有功能
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IFunction> GetAll()
        {
            return _functionDict.Values;
        }

        /// <summary>
        /// 尝试获取功能
        /// </summary>
        /// <param name="functionName"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static bool TryGetFunction(string functionName, out IFunction? function)
        {
            return _functionDict.TryGetValue(functionName, out function);
        }

        /// <summary>
        /// 尝试获取功能
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        public static bool TryGetFunction<TFunction>(out TFunction? function) where TFunction : IFunction
        {
            if(_functionDict.TryGetValue(typeof(TFunction).Name, out var _function))
            {
                function = (TFunction)_function;
               return true;
            }
            else
            {
                function = default;
                return false;
            }
        }
    }
}