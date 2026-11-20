using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using AssetCare.Model.ViewModels;
using Newtonsoft.Json;
using NLog;

namespace AssetCare.Common
{
    public class PageRulesLoader
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const string CacheKey = "AssetCarePageRules";

        private const double CacheHours = 8;

        private const string FileName = "PageRules.json";

        public static void LoadPagesRules()
        {
            try
            {
                PageRulesModel pagesRules;

                var fileURL = string.Format("{0}/{1}", "~", FileName);

                var filePath = System.Web.Hosting.HostingEnvironment.MapPath(fileURL);

                logger.Info($"PageRulesLoader file path : {filePath}");

                using (var file = File.OpenText(filePath))
                {
                    var serializer = new JsonSerializer();
                    pagesRules = (PageRulesModel)serializer.Deserialize(file, typeof(PageRulesModel));
                }

                HttpRuntime.Cache.Insert(CacheKey, pagesRules, new CacheDependency(filePath), 
                                         DateTime.Now.AddHours(CacheHours), Cache.NoSlidingExpiration);

                logger.Info($"Add json object into cache key {CacheKey} at {DateTime.Now:dd/MM/yyyy hh:mm:ss}");
            }
            catch (Exception e)
            {
                logger.Error(e, $"Loading {FileName} failed: {e.Message} \n {e.InnerException} \n {e.StackTrace}");
                throw new Exception($"Loading {FileName} failed", e.InnerException);
            }
        }

        public static PageRulesModel ReadPageRules()
        {
            var isLoadFromFile = false;
            if (HttpRuntime.Cache.Get(CacheKey) == null)
            {
                LoadPagesRules();
                isLoadFromFile = true;
            }
          
            var result = (PageRulesModel)(HttpRuntime.Cache.Get(CacheKey));

            var count = 0;
            if (result?.Pages != null)
            {
                count = result.Pages.Count;
            }

            if (isLoadFromFile)
                logger.Info($"Read page rules items : {count}");

            return result;
        }

        public static Rules GetPageRules(string pageName)
        {
            var pageRules = ReadPageRules();
            if (pageRules != null)
            {
                if (pageRules.Pages.ContainsKey(pageName))
                    return (Rules)pageRules.Pages[pageName];
            }

            logger.Info($"Canot read page rules for page name : {pageName}");
            return null;
        }

        public static string GetRolesByPages(params string[] pageNames)
        {
            IList<Rules> rules = GetRulesByPages(pageNames);
            List<string> roles = new List<string>();
            foreach (var rule in rules)
            {
                roles.AddRange(rule.ReadOnly.Split(new char[] { ',' }));
                roles.AddRange(rule.Edit.Split(new char[] { ',' }));
                roles.AddRange(rule.Add.Split(new char[] {','}));
                roles.AddRange(rule.Delete.Split(new char[] { ',' }));
            }
            return string.Join(",", roles.Distinct().OrderBy(s => s).ToArray());
        }
        private static IList<Rules> GetRulesByPages(params string[] pageNames)
        {
            List<Rules> rules = new List<Rules>();
            foreach(var pageName in pageNames )
            {
                rules.Add(GetPageRules(pageName));
            }
            return rules;
        }
    }
}