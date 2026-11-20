using System;
using System.IO;
using System.Web;
using System.Web.Caching;
using AssetCare.Model.ViewModels;
using Newtonsoft.Json;
using NLog;

namespace AssetCare.Common
{
    public class ApiRulesLoader
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const string CacheKey = "AssetCareApiRules";

        private const double CacheHours = 8;

        private const string FileName = "ApiRules.json";

        public static void LoadApisRules()
        {
            try
            {
                ApiRulesModel apisRules;

                var fileURL = string.Format("{0}/{1}", "~", FileName);

                var filePath = System.Web.Hosting.HostingEnvironment.MapPath(fileURL);

                logger.Info($"ApiRulesLoader file path : {filePath}");

                using (var file = File.OpenText(filePath))
                {
                    var serializer = new JsonSerializer();
                    apisRules = (ApiRulesModel)serializer.Deserialize(file, typeof(ApiRulesModel));
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

        public static ApiRulesModel ReadApiRules()
        {
            var isLoadFromFile = false;
            if (HttpRuntime.Cache.Get(CacheKey) == null)
            {
                LoadApisRules();
                isLoadFromFile = true;
            }

            var result = (ApiRulesModel)(HttpRuntime.Cache.Get(CacheKey));

            var count = 0;
            if (result?.Apis != null)
            {
                count = result.Apis.Count;
            }

            if (isLoadFromFile)
                logger.Info($"Read api rules items : {count}");

            return result;
        }

        public static string GetApiRules(string apiName)
        {
            var apiRules = ReadApiRules();
            if (apiRules != null)
            {
                if (apiRules.Apis.ContainsKey(apiName))
                    return (string)apiRules.Apis[apiName];
            }

            logger.Info($"Canot read api rules for api name : {apiName}");
            return null;
        }

        public static string GetMvcControllerRules(string apiName)
        {
            var mvcApiNameKey = $"{apiName}_MVC";

            var apiRules = ReadApiRules();
            if (apiRules != null)
            {
                if (apiRules.Apis.ContainsKey(mvcApiNameKey))
                    return (string)apiRules.Apis[mvcApiNameKey];
            }

            logger.Info($"Canot read api rules for MVC controller API name : {mvcApiNameKey}");
            return null;
        }
    }
}