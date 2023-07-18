using System;
using System.Collections.Generic;

namespace ThirdStoreCommon.Models.Report
{
    public partial class PriceCompareReport:BaseEntity
    {
        public PriceCompareReport()
        {
            
        }

        public DateTime CreateTime { get; set; }
        public decimal ListedPrice { get; set; }
        public decimal DSPrice { get; set; }
        public string SKU { get; set; }
        public string Ref1 { get; set; }
        public string Inspectors { get; set; }
        public string Location { get; set; }

    }
}
