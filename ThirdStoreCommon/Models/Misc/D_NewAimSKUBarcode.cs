using System;
using System.Collections.Generic;

namespace ThirdStoreCommon.Models.Misc
{
    public partial class D_NewAimSKUBarcode : BaseEntity
    {
        public string SKU { get; set; }
        public string AlternateSKU1 { get; set; }
        public string AlternateSKU2 { get; set; }

        public System.DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime EditTime { get; set; }
        public string EditBy { get; set; }
    }
}
