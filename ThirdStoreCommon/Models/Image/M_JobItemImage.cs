using System;
using System.Collections.Generic;
using ThirdStoreCommon.Models.JobItem;

namespace ThirdStoreCommon.Models.Image
{
    public partial class M_JobItemImage : BaseEntity
    {
        public int JobItemID { get; set; }
        public int ImageID { get; set; }
        public int DisplayOrder { get; set; }
        public int StatusID { get; set; }
        public System.DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime EditTime { get; set; }
        public string EditBy { get; set; }
        public virtual D_Image Image { get; set; }
        public virtual D_JobItem JobItem { get; set; }
    }
}
