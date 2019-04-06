using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace CoreFaces.KendoGrid.QueryBuilder.Mysql
{
    public static class FilterHelper
    {
        public static QueryView SqlBuilder(View filters, string sql, string defaultOrderByColumnName, bool isQueryDatatable)
        {
            QueryView queryView = new QueryView();
            var sortExpression = "";
            try
            {
                sortExpression = filters.Sort == null ? string.Empty : string.Join(",", filters.Sort.Select(item => item.GetExpression()));
            }
            catch (Exception)
            {
            }
            Tuple<string, List<MySqlParameter>> tuppleWhere = null;

            if (filters.Filter != null)
            {
                tuppleWhere = filters.Filter.GetExpression(isQueryDatatable);
                if (tuppleWhere.Item1 != "" && tuppleWhere.Item1 != null)
                {
                    queryView.filterExpression = tuppleWhere.Item1;
                }

                if (tuppleWhere.Item2.Count > 0)
                {
                    queryView.filterParameters = tuppleWhere.Item2;
                }
            }

            if (queryView.filterExpression != "")
            {
                queryView.filterExpression = " where " + queryView.filterExpression;
            }

            if (sortExpression == "")
            {
                sortExpression = "ORDER BY " + defaultOrderByColumnName + " DESC";
            }
            else
            {
                sortExpression = " ORDER BY " + sortExpression;
            }

            sql = string.Format(sql, queryView.filterExpression, sortExpression, filters.Skip, filters.Take);
            queryView.sql = sql;
            return queryView;
        }

        public static View FilterInit(View filters)
        {
            bool isSetLogic = false;
            if (filters == null)
            {

                filters = new View();
                if (filters.Filter.Filters == null)
                {
                    filters.Filter.Filters = new List<Filter>();
                }
            }
            else
            {
                if (filters.Filter != null)
                {

                }
                else
                {
                    filters.Filter = new Filter();
                    filters.Filter.Filters = new List<Filter>();
                    isSetLogic = true;
                }
            }
            if (isSetLogic)
                filters.Filter.Logic = "And";

            return filters;
        }
    }
}
