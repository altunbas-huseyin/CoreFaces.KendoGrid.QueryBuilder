using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CoreFaces.KendoGrid.QueryBuilder.Mssql
{
    [DataContract, Serializable]
    public class Sort
    {
        /// <summary>
        /// Gets or sets the direction.
        /// </summary>
        /// <value>
        /// The direction.
        /// </value>
        [DataMember(Name = "dir")]
        public string Direction { get; set; }

        /// <summary>
        /// Gets or sets the field.
        /// </summary>
        /// <value>
        /// The field.
        /// </value>
        [DataMember(Name = "field")]
        public string Field { get; set; }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <returns>
        /// The expression.
        /// </returns>
        public string GetExpression()
        {
            return this.Field + " " + this.Direction;
        }
    }

}
