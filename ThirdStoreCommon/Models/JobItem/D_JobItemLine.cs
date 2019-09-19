using System;
using System.Collections.Generic;
using ThirdStoreCommon.Models.Item;

namespace ThirdStoreCommon.Models.JobItem
{
    public partial class D_JobItemLine : BaseEntity
    {
        public D_JobItemLine()
        {

        }

        public int HeaderID { get; set; }
        public int ItemID { get; set; }
        public string SKU { get; set; }
        public int Qty { get; set; }
        public string SupplierRef { get; set; }
        public bool IsOrginalPackage { get; set; }
        public decimal Weight { get; set; }
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public decimal CubicWeight { get; set; }
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }


        public System.DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime EditTime { get; set; }
        public string EditBy { get; set; }

        public virtual D_JobItem JobItem { get; set; }
        public virtual D_Item Item { get; set; }
    }
}
