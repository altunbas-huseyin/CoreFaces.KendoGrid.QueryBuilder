using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CoreFaces.KendoGrid.QueryBuilder.Mssql
{
    [DataContract, Serializable]
    public class Grid
    {
        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        [DataMember(Name = "data")]
        public string Data { get; set; }

        /// <summary>
        /// Gets or sets the total.
        /// </summary>
        /// <value>
        /// The total.
        /// </value>
        [DataMember(Name = "total")]
        public int Total { get; set; }
    }
}
