using AssetCare.Model.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace AssetCare.Common
{
    public static class DataTableHelper
    {
        public static DataTableFilterModel TransformDataTablePostModel(DataTableAjaxPostModel model)
        {
            var searchBy = (model.search != null) && model.search.value != null ? model.search.value : "";
            var take = model.length;
            var skip = model.start;
            var fieldsToSearch = model.columns.Select(p => p.data).Distinct().ToArray();

            string sortBy = "";
            string sortDir = Constants.ASCENDING;

            if (model.order != null)
            {
                sortBy = model.columns[model.order[0].column].data;
                sortDir = model.order[0].dir.ToLower() == Constants.ASCENDING_SHORT ? Constants.ASCENDING : Constants.DESCENDING;
            }

            var columnSearches = model.columns.Where(p => p.search.value != null).Select(p => new { p.data, p.search.value }).Distinct().ToDictionary(p1 => p1.data, p2 => p2.value.ToLower());

            return new DataTableFilterModel
            {
                SearchBy = searchBy,
                ColumnSearches = columnSearches,
                Take = take,
                Skip = skip,
                SortBy = sortBy,
                SortDir = sortDir
            };
        }

        public static DataTableFilterModel<T> TransformDataTablePostModel<T>(DataTableAjaxPostModel model)
        {
            var searchBy = (model.search != null) && model.search.value != null ? model.search.value : "";
            var take = model.length;
            var skip = model.start;

            string sortBy = "";
            string sortDir = Constants.ASCENDING;

            if (model.order != null && model.order.Any())
            {
                var order = model.order[0];
                if (model.columns != null && order.column >= 0 && order.column < model.columns.Count)
                {
                    sortBy = model.columns[order.column].data;
                    sortDir = !string.IsNullOrWhiteSpace(order.dir) && order.dir.ToLower() == Constants.ASCENDING_SHORT ? Constants.ASCENDING : Constants.DESCENDING;
                }
            }

            var columnSearches = (model.columns ?? new List<Column>())
                .Where(p => p.search != null && p.search.value != null)
                .Select(p => new { p.data, p.search.value })
                .Distinct()
                .ToDictionary(p1 => p1.data, p2 => p2.value.ToLower());
            
            var filterModel = new DataTableFilterModel<T>
            {
                SearchBy = searchBy,
                Take = take,
                Skip = skip,
                SortBy = sortBy,
                SortDir = sortDir,
                ColumnSearches = columnSearches 
            };

            filterModel.Sanitize();
            return filterModel;
        }
    }
}