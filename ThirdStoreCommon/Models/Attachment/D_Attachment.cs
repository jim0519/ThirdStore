using System;
using System.Collections.Generic;

namespace ThirdStoreCommon.Models.Attachment
{
    public partial class D_Attachment : BaseEntity
    {
        public D_Attachment()
        {

        }

        public string Name { get; set; }
        public string LocalPath { get; set; }
        public System.DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime EditTime { get; set; }
        public string EditBy { get; set; }
    }
}
