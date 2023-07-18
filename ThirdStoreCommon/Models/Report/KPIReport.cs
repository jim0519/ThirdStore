using System;
using System.Collections.Generic;

namespace ThirdStoreCommon.Models.Report
{
    public partial class KPIReport
    {
        public KPIReport()
        {
            
        }

        public DateTime CreateDate { get; set; }
        public int StaffNum { get; set; }
        public decimal TotalValue { get; set; }
        public decimal AVGValue { get; set; }
        public int TotalQty { get; set; }
        public decimal AVGQty { get; set; }
        public int TotalQtyD { get; set; }
        public int TotalQtyLocFRS { get; set; }
        public int TotalQtyLocT { get; set; }
        public int TotalQtyLocOther { get; set; }
        public decimal QMW { get; set; }
        public decimal QMWD { get; set; }
        public decimal AVGQMW { get; set; }
        public decimal FaultyRate { get; set; }
    }
}
