using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Caching;
using AssetCare.Model.ViewModels;
using Newtonsoft.Json;
using NLog;

namespace AssetCare.Common
{
    public static class GlobalNotification
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const string CacheKey = "AssetCareGlobalNotification";

        private const double CacheHours = 8;

        private const string NotificationBaseFolder = "~/Content/Notifications";

        private const string FileName = "GlobalNotification.json";

        public static List<GlobalNotificationModel> Notifications
        {
            get
            {
                List<GlobalNotificationModel> items;
                if (HttpRuntime.Cache.Get(CacheKey) == null)
                {
                    var notifications = new List<GlobalNotificationModel>();
                    var fileURL = string.Format("{0}/{1}", NotificationBaseFolder, FileName);

                    var filePath = System.Web.Hosting.HostingEnvironment.MapPath(fileURL);
                    using (var file = File.OpenText(filePath))
                    {
                        var serializer = new JsonSerializer();
                        items = (List<GlobalNotificationModel>)serializer.Deserialize(file, typeof(List<GlobalNotificationModel>));
                    }
                    HttpRuntime.Cache.Insert(CacheKey, items, new CacheDependency(filePath),
                        DateTime.Now.AddHours(CacheHours), Cache.NoSlidingExpiration);
                }

                items = (List<GlobalNotificationModel>)(HttpRuntime.Cache.Get(CacheKey));

                return items;
            }
        }
    }
}