using System;
using System.Collections.Generic;

namespace ThirdStoreCommon.Models.Order
{
    public partial class D_Order_Line : BaseEntity
    {
        public D_Order_Line()
        {

        }

        public int HeaderID { get; set; }
        public string SKU { get; set; }
        public int Qty { get; set; }
        public decimal ItemPrice { get; set; }
        public decimal SubTotal { get; set; }
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }
        public string Ref6 { get; set; }
        public string Ref7 { get; set; }
        public string Ref8 { get; set; }
        public string Ref9 { get; set; }
        public string Ref10 { get; set; }
        public System.DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime EditTime { get; set; }
        public string EditBy { get; set; }
        public virtual D_Order_Header Order { get; set; }
    }
}
