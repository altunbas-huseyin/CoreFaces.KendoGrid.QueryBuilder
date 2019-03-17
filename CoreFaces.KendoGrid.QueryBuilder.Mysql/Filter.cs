using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CoreFaces.KendoGrid.QueryBuilder.Mysql
{
    /// <summary>
    /// Represents a filter.
    /// </summary>
    [DataContract, Serializable]
    public class Filter
    {
        /// <summary>
        /// The templates
        /// </summary>
        private static readonly IDictionary<string, string> Templates = new Dictionary<string, string>
    {
        { "eq", "{0} = {1}" },
        { "neq", "{0} <> {1}" },
        { "lt", "{0} < {1}" },
        { "lte", "{0} <= {1}" },
        { "gt", "{0} > {1}" },
        { "gte", "{0} >= {1}" },
        { "startswith", "{0} like CONCAT({1}, '%')" },
        { "endswith", "{0} like CONCAT('%', {1})" },
        { "contains", "{0} like CONCAT('%', {1}, '%')" },
        { "doesnotcontain", "{0} not like CONCAT('%', {1}, '%')" },
        { "in", "{0} in ({1})" },

        { "eq_datatable", "{0} = {1}" },
        { "neq_datatable", "{0} <> {1}" },
        { "lt_datatable", "{0} < {1}" },
        { "lte_datatable", "{0} <= {1}" },
        { "gt_datatable", "{0} > {1}" },
        { "gte_datatable", "{0} >= {1}" },
        { "startswith_datatable", "({0} like ''+{1}+'%')" },
        { "endswith_datatable", "({0} like '%'+{1}+'')" },
        { "contains_datatable", "({0} like '%'+{1}+'%')" },
        { "doesnotcontain_datatable", "({0} not like '%'+{1}+'%')" },
    };

        /// <summary>
        /// Gets or sets the field.
        /// </summary>
        /// <value>
        /// The field.
        /// </value>
        [DataMember(Name = "field")]
        public string Field { get; set; }

        /// <summary>
        /// Gets or sets the filters.
        /// </summary>
        /// <value>
        /// The filters.
        /// </value>
        [DataMember(Name = "filters")]
        public List<Filter> Filters { get; set; }

        /// <summary>
        /// Gets or sets the logic.
        /// </summary>
        /// <value>
        /// The logic.
        /// </value>
        [DataMember(Name = "logic")]
        public string Logic { get; set; }

        /// <summary>
        /// Gets or sets the operator.
        /// </summary>
        /// <value>
        /// The operator.
        /// </value>
        [DataMember(Name = "operator")]
        public string Operator { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        [DataMember(Name = "value")]
        public object Value { get; set; } = "";


        [DataMember(Name = "MySqlParameterName")]
        public string MySqlParameterName { get; set; } = "";

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <returns>
        /// The expression.
        /// </returns>
        public Tuple<string, List<MySqlParameter>> GetExpression(bool isQueryDatatable)
        {
            var result = this.ExpressionInit(this.Filters);
            result = this.FixDuplicateColumnFilter(result, "siparis_statu");
            if (isQueryDatatable)
            { result = QueryDatatableOptimize(result); }
            return this.GetExpression(result, this.Logic);
        }

        /// <summary>
        /// Called when deserialized.
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            //if (this.Field != null)
            //{
            //    if (this.Field == "id")
            //    { this.Field = "Id"; }

            //    this.Field = ConvertToCamelCase(this.Field);
            //}



            if (this.Value != null)
            {
                try
                {
                    var value = this.Value.ToString();
                    DateTime temp;
                    if (DateTime.TryParse(value, out temp))
                    {
                        //// The digits represent the milliseconds since the start of the Unix epoch
                        //var milliseconds = long.Parse(value.Substring(6, 13));
                        //var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                        //// This date format works with the data table select statement
                        //this.Value = unixEpoch.AddMilliseconds(milliseconds).ToString("yyyy-MM-dd");
                        this.Value = DateTime.Parse(value).AddDays(1).ToString("yyyy.MM.dd");//+ " 23:59";
                    }

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }



        private List<Filter> QueryDatatableOptimize(List<Filter> filters)
        {

            foreach (Filter filter in filters)
            {
                if (!string.IsNullOrEmpty(filter.Operator))
                {
                    filter.Operator = filter.Operator + "_datatable";
                }

                if (filter.Filters != null)
                {
                    this.QueryDatatableOptimize(filter.Filters);
                }
            }

            return filters;
        }

        private List<Filter> FixDuplicateColumnFilter(List<Filter> filters, string fieldName)
        {
            List<Filter> tempStatusFilter = filters.Where(p => p.Field == fieldName).ToList();
            if (tempStatusFilter.Count > 0)
            {
                filters.RemoveAll(p => p.Field == fieldName);
                Filter filterTemp = tempStatusFilter.GroupBy(p => p.Field).Select(grp => grp.Last()).FirstOrDefault();
                filters.Add(filterTemp);
            }


            for (int i = 0; i < filters.Count; i++)
            {
                if (filters[i].Filters != null)
                {
                    List<Filter> temp = filters[i].Filters.Where(p => p.Field == fieldName).ToList();
                    if (temp.Count > 0)
                    {
                        filters[i].Filters.RemoveAll(p => p.Field == fieldName);
                        Filter filterTemp = temp.GroupBy(p => p.Field).Select(grp => grp.Last()).FirstOrDefault();
                        filters[i].Filters.Add(filterTemp);
                    }
                }
            }


            return filters;
        }

        private List<Filter> ExpressionInit(List<Filter> filters)
        {
            //for (int i = 0; i < filters.Count(); i++)
            //{
            //    if (filters.ToList()[i].Filters != null)
            //    {
            //        if (filters.ToList()[i].Filters.Where(p => p.Field == "created").Count() == 2)
            //        {
            //            filters.ToList()[i].Filters[1].Field = "created";
            //            filters.ToList()[i].Filters[1].MySqlParameterName = "created_1";
            //        }
            //    }

            //}


            List<Filter> tempFiltersCreate = filters.Where(p => p.Operator == "lte" || p.Operator == "gte" || p.Field == "create").ToList();
            if (tempFiltersCreate.Count == 2)
            {
                filters.Where(p => p.Operator == "lte" || p.Operator == "gte" || p.Field == "create").ToList()[1].MySqlParameterName = "created_1";
            }


            List<Filter> tempFiltersDate = filters.Where(p => p.Operator == "lte" || p.Operator == "gte" || p.Field == "date").ToList();
            if (tempFiltersDate.Count == 2)
            {
                filters.Where(p => p.Operator == "lte" || p.Operator == "gte" || p.Field == "date").ToList()[1].MySqlParameterName = "date_1";
            }


            foreach (Filter filter in filters)
            {
                if (filter.Filters != null)
                {
                    if (filter.Filters.Count > 1)
                        this.ExpressionInit(filter.Filters);
                }
            }




            return filters;
        }


        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <param name="logic">The logic.</param>
        /// <returns>
        /// The expression.
        /// </returns>
        private Tuple<string, List<MySqlParameter>> GetExpression(List<Filter> filters, string logic)
        {
            string result = string.Empty;
            List<MySqlParameter> resultParams = new List<MySqlParameter>();
            if (filters != null && filters.Any<Filter>() && !string.IsNullOrWhiteSpace(logic))
            {
                var list = new List<string>();
                List<MySqlParameter> listParams = new List<MySqlParameter>();

                foreach (Filter _filter in filters)
                {
                    Filter filter = _filter;
                    filter = FilterInit(filter);
                    if (!string.IsNullOrWhiteSpace(filter.Field))
                    {

                        string template = Templates[filter.Operator];
                        string value = filter.Value.ToString();

                        if (filter.Operator == "in")
                        {
                            list.Add(string.Format(template, filter.Field, value));
                        }
                        else
                        {

                            list.Add(string.Format(template, filter.Field, "@" + filter.MySqlParameterName));
                            listParams.Add(new MySqlParameter("@" + filter.MySqlParameterName, value));
                        }

                    }

                    //Reqursive Call
                    if (filter.Filters != null)
                    {
                        var childFilterResult = this.GetExpression(filter.Filters, filter.Logic);
                        list.Add(childFilterResult.Item1);
                        foreach (var item in childFilterResult.Item2)
                        {
                            listParams.Add(item);
                        }
                    }
                }

                result = "(" + string.Join(" " + logic + " ", list) + ")";
                foreach (var item in listParams)
                {
                    resultParams.Add(item);
                }
            }

            return Tuple.Create(result, resultParams);
        }

        private Filter FilterInit(Filter filter)
        {

            if (!filter.MySqlParameterName.Contains("_1"))
            {
                filter.MySqlParameterName = filter.Field;
            }
            else
            {
                filter.Value = DateTime.Parse(filter.Value.ToString()).ToString("yyyy.MM.dd") + " 23:59:59:59";
            }

            if (filter.Field == "statusText")
            {
                filter.Field = "status";
                filter.MySqlParameterName = filter.MySqlParameterName + "_1";

                if (filter.Value.ToString() == "Yeni")
                    filter.Value = 0;
                else if (filter.Value.ToString() == "Aktif")
                    filter.Value = 1;
                else if (filter.Value.ToString() == "Pasif")
                    filter.Value = 2;
                else if (filter.Value.ToString() == "Onaylandı")
                    filter.Value = 3;
                else if (filter.Value.ToString() == "Reddedildi")
                    filter.Value = 4;
                else if (filter.Value.ToString() == "Onay Bekliyor")
                    filter.Value = 5;
            }

            if (filter.Field == "userTypeText")
            {
                filter.Field = "userType";
                filter.MySqlParameterName = filter.MySqlParameterName + "_1";

                if (filter.Value.ToString() == "Bayi")
                    filter.Value = 1;
                else if (filter.Value.ToString() == "Toptancı")
                    filter.Value = 2;
                else if (filter.Value.ToString() == "Tali Bayi")
                    filter.Value = 3;

            }

            if (filter.Field == "transactionTypeText")
            {

                filter.Field = "transactionType";
                filter.MySqlParameterName = filter.MySqlParameterName + "_1";

                if (filter.Value.ToString() == "Faturadan kazanılan puan.")
                    filter.Value = 1;
                else if (filter.Value.ToString() == "Fatura iptaline istinaden silinen puan.")
                    filter.Value = 2;
                else if (filter.Value.ToString() == "Eğitimden kazanılan puan.")
                    filter.Value = 3;
                else if (filter.Value.ToString() == "Manuel yüklenen puan.")
                    filter.Value = 4;
                else if (filter.Value.ToString() == "Manuel silinen puan.")
                    filter.Value = 5;
            }




            return filter;
        }

        private string ConvertToCamelCase(string phrase)
        {
            string[] splittedPhrase = phrase.Split(' ', '-', '.');
            var sb = new StringBuilder();
            //sb.Append(splittedPhrase[0].ToLower());
            //splittedPhrase[0] = string.Empty;

            foreach (String s in splittedPhrase)
            {
                char[] splittedPhraseChars = s.ToCharArray();
                if (splittedPhraseChars.Length > 0)
                {
                    splittedPhraseChars[0] = ((new String(splittedPhraseChars[0], 1)).ToUpper().ToCharArray())[0];
                }
                sb.Append(new String(splittedPhraseChars));
            }
            return sb.ToString();
        }

    }
}
