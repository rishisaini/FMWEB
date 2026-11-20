using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace AssetCare.Common
{
    public static class UriExtension
    {
        public static string Domain(this Uri uri)
        {
            return uri.Authority;
            //var domain = ConfigurationManager.AppSettings["AssetCareSettings:Domain"];
            //return domain ?? uri.Authority;
        }
    }
}