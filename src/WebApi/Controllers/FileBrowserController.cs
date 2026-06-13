//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Text;
//using System.Threading.Tasks;
//using XMLData.Item;

//namespace SdtdServerKit.WebApi.Controllers
//{
//    /// <summary>
//    /// FileBrowser
//    /// </summary>
//    [Authorize]
//    [RoutePrefix("api/FileBrowser")]
//    public class FileBrowserController : ApiController
//    { 
//        /// <summary>
//        /// Get file list
//        /// </summary>
//        /// <returns></returns>
//        [HttpGet]
//        [Route("Token")]
//        public string GetToken()
//        {
//            string data = "{\"username\":\"" + ModApi.AppSettings.UserName + "\",\"password\":\"" + ModApi.AppSettings.Password + "\"}";
//            byte[] byteArray = Encoding.UTF8.GetBytes(data);

//            string url = string.Format("http://127.0.0.1:{0}/api/login", ModApi.AppSettings.FileBrowserPort);
//            var request = (HttpWebRequest)WebRequest.Create(url);
//            request.Method = "POST";

//            request.ContentType = "application/json";
//            request.ContentLength = byteArray.Length;
//            using (Stream reqStream = request.GetRequestStream())
//            {
//                reqStream.Write(byteArray, 0, byteArray.Length);
//            }

//            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
//            {
//                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
//                {
//                    string responseData = reader.ReadToEnd();
//                    return responseData;
//                }
//            }
//        }
//    }
//}
