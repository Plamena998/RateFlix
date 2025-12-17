using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateFlix.Core
{
    public class AppOptions
    {
        public const string ApiParameters = "AppOptions";
        public string ApiKey { get; set; } = string.Empty;  
        public string Url { get; set; } = string.Empty;
    }
}
