using System;
using System.Collections.Generic;
using ThirdStoreCommon.Models.Item;

namespace ThirdStoreCommon.Models.Attachment
{
    public partial class M_ItemAttachment : BaseEntity
    {
        public int ItemID { get; set; }
        public int AttachmentID { get; set; }
        public int DisplayOrder { get; set; }
        public int StatusID { get; set; }
        public string Notes { get; set; }
        public System.DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime EditTime { get; set; }
        public string EditBy { get; set; }
        public virtual D_Attachment Attachment { get; set; }
        public virtual D_Item Item { get; set; }
    }
}
