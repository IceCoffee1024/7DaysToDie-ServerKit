using SdtdServerKit.Data.IRepositories;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.MuteCommandAreas;

namespace SdtdServerKit.Functions
{
    /// <summary>
    /// 区域命令禁用功能
    /// </summary>
    public class MuteCommandArea : FunctionBase<MuteCommandAreaSettings>
    {
        private readonly IMuteCommandAreaRepository _repository;

        /// <summary>
        /// 构造函数
        /// </summary>
        public MuteCommandArea(IMuteCommandAreaRepository repository)
        {
            _repository = repository;
        }

        protected override void OnEnableFunction()
        {
            // 初始化管理器，从数据库加载数据
            MuteCommandManager.Initialize(_repository);

            CustomLogger.Debug("区域命令禁用功能：已启用（拦截聊天命令和控制台命令）");
        }

        protected override void OnDisableFunction()
        {
            CustomLogger.Debug("区域命令禁用功能：已禁用");
        }
    }
}
