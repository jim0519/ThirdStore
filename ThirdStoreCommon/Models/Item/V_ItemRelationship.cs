using System;
using System.Collections.Generic;

namespace ThirdStoreCommon.Models.Item
{
    public partial class V_ItemRelationship : BaseEntity
    {
        public int ItemID { get; set; }
        public string SKU { get; set; }
        public int ItemType { get; set; }
        public int Qty { get; set; }
        public int BottomItemID { get; set; }
        public string BottomSKU { get; set; }
    }
}
