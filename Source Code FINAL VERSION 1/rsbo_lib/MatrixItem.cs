using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rsbo_lib
{
    [Serializable]
    public class MatrixItem
    {
        public string ClusterID { get; set; }
        /// <summary>
        /// User ID / MetaItemSource
        /// </summary>
        public string Row { get; set; }
        /// <summary>
        /// Item ID / MetaItemDestination
        /// </summary>
        public string Column { get; set; }
        /// <summary>
        /// Rate /  Confident / WEIGHT
        /// </summary>
        public double Cell { get; set; }
    }
}
