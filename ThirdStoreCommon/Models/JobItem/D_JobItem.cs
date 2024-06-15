using System;
using System.Collections.Generic;
using ThirdStoreCommon.Models.Attachment;
using ThirdStoreCommon.Models.Image;

namespace ThirdStoreCommon.Models.JobItem
{
    public partial class D_JobItem : BaseEntity
    {
        public D_JobItem()
        {
            this.JobItemLines = new List<D_JobItemLine>();
            this.JobItemImages = new List<M_JobItemImage>();
            this.JobItemAttachments = new List<M_JobItemAttachment>();
        }

        public System.DateTime JobItemCreateTime { get; set; }
        public int Type { get; set; }
        public int StatusID { get; set; }
        public int ConditionID { get; set; }      
        public string ItemName { get; set; }
        public string ItemDetail { get; set; }
        public decimal ItemPrice { get; set; }
        public string Location { get; set; }
        public string DesignatedSKU { get; set; }
        public DateTime? ShipTime { get; set; }
        public string TrackingNumber { get; set; }
        public string Ref1 { get; set; }//JobItemReference
        public string Ref2 { get; set; }//Inspectors
        public string Ref3 { get; set; }//CS Note
        public string Ref4 { get; set; }//Previous Location
        public string Ref5 { get; set; }//Review Comments
        public string Note { get; set; }
        public decimal PricePercentage { get; set; } = 1;
        public DateTime? StocktakeTime { get; set; }
        //public bool NeedReview { get; set; }
        public int ReviewStatus { get; set; }
        public int Qty { get; set; }


        public System.DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime EditTime { get; set; }
        public string EditBy { get; set; }

        public virtual ICollection<D_JobItemLine> JobItemLines { get; set; }
        public virtual ICollection<M_JobItemImage> JobItemImages { get; set; }
        public virtual ICollection<M_JobItemAttachment> JobItemAttachments { get; set; }
    }
}
