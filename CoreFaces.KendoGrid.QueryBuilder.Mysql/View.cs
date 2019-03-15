using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace CoreFaces.KendoGrid.QueryBuilder.Mysql
{
    public class View
    {
        public int Take { get; set; } = 20;
        public int Skip { get; set; }
        public List<Sort> Sort { get; set; }
        public Filter Filter { get; set; }
        public int PageSize { get; set; }
        public int Page { get; set; }

    }

    public class QueryView
    {
        public string filterExpression { get; set; } = "";
        public string sortExpression { get; set; } = "";
        public string sql { get; set; } = "";
        public List<MySqlParameter> filterParameters { get; set; } = new List<MySqlParameter>();
        public List<MySqlParameter> parametersParameters { get; set; } = new List<MySqlParameter>();
    }
}
