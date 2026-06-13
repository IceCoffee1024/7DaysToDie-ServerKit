using System.Web.Http;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// 首次安装初始化（设置管理员账号密码）
    /// </summary>
    [AllowAnonymous]
    [RoutePrefix("api/Setup")]
    public class SetupController : ApiController
    {
        /// <summary>
        /// 查询是否需要进行首次初始化（设置账号密码）。
        /// </summary>
        [HttpGet]
        [Route("Status")]
        [ResponseType(typeof(SetupStatusResult))]
        public IHttpActionResult GetStatus()
        {
            return Ok(new SetupStatusResult
            {
                NeedSetup = ModApi.NeedInitialize
            });
        }

        /// <summary>
        /// 设置管理员账号密码（仅首次安装、尚未初始化时可用）。
        /// </summary>
        [HttpPost]
        [Route("")]
        public IHttpActionResult Setup([FromBody] SetupRequest model)
        {
            // 已经初始化过则拒绝，防止越权改密
            if (!ModApi.NeedInitialize)
            {
                return BadRequest("已完成初始化，无法重复设置账号密码。");
            }

            if (model == null)
            {
                return BadRequest("请求体不能为空。");
            }

            string userName = (model.UserName ?? string.Empty).Trim();
            string password = model.Password ?? string.Empty;

            if (string.IsNullOrWhiteSpace(userName) || userName.Length < 3 || userName.Length > 32)
            {
                return BadRequest("用户名长度需在 3 到 32 个字符之间。");
            }

            if (string.IsNullOrEmpty(password) || password.Length < 6 || password.Length > 64)
            {
                return BadRequest("密码长度需在 6 到 64 个字符之间。");
            }

            try
            {
                ModApi.AppSettings.UserName = userName;
                ModApi.AppSettings.Password = password;
                ModApi.SaveAppSettings();

                CustomLogger.Debug("首次安装初始化完成：已设置管理员账号。");
                return Ok();
            }
            catch (System.Exception ex)
            {
                CustomLogger.Error(ex, "首次安装初始化：保存账号密码失败");
                return InternalServerError(ex);
            }
        }
    }

    /// <summary>
    /// 初始化状态结果
    /// </summary>
    public class SetupStatusResult
    {
        /// <summary>
        /// 是否需要进行首次初始化（设置账号密码）
        /// </summary>
        public bool NeedSetup { get; set; }
    }

    /// <summary>
    /// 设置账号密码请求
    /// </summary>
    public class SetupRequest
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string? Password { get; set; }
    }
}
