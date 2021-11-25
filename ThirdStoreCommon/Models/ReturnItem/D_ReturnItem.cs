using System;
using System.Collections.Generic;

namespace ThirdStoreCommon.Models.ReturnItem
{
    public partial class D_ReturnItem : BaseEntity
    {
        public D_ReturnItem()
        {
            this.ReturnItemLines = new List<D_ReturnItemLine>();
        }

        public int StatusID { get; set; }
        public string Note { get; set; }
        public string DesignatedSKU { get; set; }
        public string TrackingNumber { get; set; }
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }
       
        public System.DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime EditTime { get; set; }
        public string EditBy { get; set; }

        public virtual ICollection<D_ReturnItemLine> ReturnItemLines { get; set; }
    }
}
