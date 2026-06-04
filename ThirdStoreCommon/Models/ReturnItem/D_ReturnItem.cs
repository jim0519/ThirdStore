using System;
using System.Collections.Generic;
using ThirdStoreCommon.Models.Image;
using ThirdStoreCommon.Models.JobItem;

namespace ThirdStoreCommon.Models.ReturnItem
{
    public partial class D_ReturnItem : BaseEntity
    {
        public D_ReturnItem()
        {
            this.ReturnItemLines = new List<D_ReturnItemLine>();
            this.ReturnItemImages = new List<M_ReturnItemImage>();
        }

        public int StatusID { get; set; }
        public string Note { get; set; }
        public string DesignatedSKU { get; set; }
        public string TrackingNumber { get; set; }
        public int SupplierID { get; set; }
        public string CarrierName { get; set; }
        public bool NOP { get; set; }
        public bool FullSet { get; set; }
        public string Location { get; set; }
        public int ReturnTypeID { get; set; }
        public DateTime ReceivedDate { get; set; }
        public DateTime? ProcessDate { get; set; }
        public int ConvertToJobItemID { get; set; }
        public string Ref1 { get; set; }//Record_ID
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }
       
        public System.DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime EditTime { get; set; }
        public string EditBy { get; set; }

        public virtual D_JobItem JobItem { get; set; }
        public virtual ICollection<D_ReturnItemLine> ReturnItemLines { get; set; }
        public virtual ICollection<M_ReturnItemImage> ReturnItemImages { get; set; }
    }
}
