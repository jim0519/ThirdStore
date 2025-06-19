using System;
using System.Collections.Generic;

namespace ThirdStoreCommon.Models.Report
{
    public partial class SupplierStatisticReport : BaseEntity
    {
        public SupplierStatisticReport()
        {
            
        }

        public string Supplier { get; set; }
        public int SelfStored { get; set; }
        public decimal SelfStoredTotalPrice { get; set; }
        public int Shipped { get; set; }
        public decimal ShippedTotalPrice { get; set; }
        public decimal ShippedPercentage { get; set; }
        public decimal ShippedTotalPricePercentage { get; set; }
        public int Distributed { get; set; }
        public int Total { get; set; }
        public decimal DistributedPercentage { get; set; }

    }
}
