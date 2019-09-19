using System;
using System.Collections.Generic;

namespace ThirdStoreCommon.Models.Item
{
    public partial class D_Item_Relationship : BaseEntity
    {
        public int ParentItemID { get; set; }
        public int ChildItemID { get; set; }
        public int ChildItemQty { get; set; }
        public int DisplayOrder { get; set; }


        public System.DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime EditTime { get; set; }
        public string EditBy { get; set; }

        public virtual D_Item ChildItem { get; set; }
        public virtual D_Item ParentItem { get; set; }
    }
}
