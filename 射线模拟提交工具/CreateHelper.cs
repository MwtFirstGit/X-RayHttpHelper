using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 射线模拟提交工具
{
    public class CreateHelper
    {
        public bool Check { get; internal set; }
        public string RegStr { get; internal set; }
        public bool CheckXpath { get; internal set; }
        public string XpathStr { get; internal set; }
        public bool TimerEnable { get; internal set; }
        public int ThreadNum { get; internal set; } = 1;
        public bool ProxyEnable { get; internal set; }
        public string ProxyAPI { get; internal set; }
        public bool LoopGet { get; internal set; }
    }

    public class UpdateInfo
    {
        public string name { get; set; }
        public string hash { get; set; }
        public string url { get; set; }
    }
}
