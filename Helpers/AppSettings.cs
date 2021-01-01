using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NG_Core_Auth.Helpers
{
    public class AppSettings
    {
        // this class for jwt tokens settings
        // data is exist in appsettings.json file in AppSettings section 
        public string Site { set; get; }
        public string Audience { get; set; }
        public string Secret { get; set; }
        public string ExpireTime { get; set; }
    }
}
