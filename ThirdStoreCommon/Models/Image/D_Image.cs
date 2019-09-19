using System;
using System.Collections.Generic;

namespace ThirdStoreCommon.Models.Image
{
    public partial class D_Image : BaseEntity
    {
        public D_Image()
        {

        }

        public string ImageName { get; set; }
        public string ImageLocalPath { get; set; }
        public System.DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime EditTime { get; set; }
        public string EditBy { get; set; }
    }
}
