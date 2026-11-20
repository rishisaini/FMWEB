using System;
using System.IO;
using System.Web;
using System.Web.Caching;
using AssetCare.Model.ViewModels;
using Newtonsoft.Json;
using NLog;

namespace AssetCare.Common
{
    public class ExportTableSettingsLoader : ISettingLoader
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const string CacheKey = "AssetCareExportTableSettings";

        private const double CacheHours = 8;

        private const string FileName = "ExportTableSettings.json";

        private void LoadSettings()
        {
            try
            {
                ExportTableSettingsModel apisRules;

                var fileURL = string.Format("{0}/{1}", "~", FileName);

                var filePath = System.Web.Hosting.HostingEnvironment.MapPath(fileURL);

                logger.Info($"ApiRulesLoader file path : {filePath}");

                using (var file = File.OpenText(filePath))
                {
                    var serializer = new JsonSerializer();
                    apisRules = (ExportTableSettingsModel)serializer.Deserialize(file, typeof(ExportTableSettingsModel));
                }

                HttpRuntime.Cache.Insert(CacheKey, apisRules, new CacheDependency(filePath),
                                         DateTime.Now.AddHours(CacheHours), Cache.NoSlidingExpiration);

                logger.Info($"Add json object into cache key {CacheKey} at {DateTime.Now:dd/MM/yyyy hh:mm:ss}");
            }
            catch (Exception e)
            {
                logger.Error(e, $"Loading {FileName} failed: {e.Message} \n {e.InnerException} \n {e.StackTrace}");
                throw new Exception($"Loading {FileName} failed", e.InnerException);
            }
        }

        public ExportTableSettingsModel ReadSettings()
        {
            var isLoadFromFile = false;
            if (HttpRuntime.Cache.Get(CacheKey) == null)
            {
                LoadSettings();
                isLoadFromFile = true;
            }

            var result = (ExportTableSettingsModel)(HttpRuntime.Cache.Get(CacheKey));

            var count = 0;
            if (result?.Settings != null)
            {
                count = 1;
            }

            if (isLoadFromFile)
                logger.Info($"Read Export Table Settings items : {count}");

            return result;
        }
    }
}