using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SdtdServerKit.Models
{
    public class BackupFileResult : FileResult
    {
        public string ServerVersion { get; set; }
        public string GameWorld { get; set; }
        public string GameName { get; set; }
        public int Days { get; set; }
        public int Hours { get; set; }
    }
}
