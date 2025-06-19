using System;
using System.Collections.Generic;

namespace ThirdStoreCommon.Models.ReturnItem
{
    public partial class T_CarrierTrackingRule : BaseEntity
    {
        public T_CarrierTrackingRule()
        {

        }
        public string CarrierMatchCode { get; set; }
        public int TrackingPrefixDigit { get; set; }
        public int TrackingMainDigit { get; set; }
        public int SupplierID { get; set; }
        public string CarrierName { get; set; }
        public System.DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime EditTime { get; set; }
        public string EditBy { get; set; }

    }
}
