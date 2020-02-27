using System;
using System.Collections.Generic;
using ThirdStoreCommon.Models.Image;

namespace ThirdStoreCommon.Models.Item
{
    public partial class NetoProducts:BaseEntity
    {
        public NetoProducts()
        {
            
        }

        public string NetoProductID { get; set; }
        public string SKU { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DefaultPrice { get; set; }
        public string PrimarySupplier { get; set; }
        public string Image1 { get; set; }
        public string Image2 { get; set; }
        public string Image3 { get; set; }
        public string Image4 { get; set; }
        public string Image5 { get; set; }
        public string Image6 { get; set; }
        public string Image7 { get; set; }
        public string Image8 { get; set; }
        public string Image9 { get; set; }
        public string Image10 { get; set; }
        public string Image11 { get; set; }
        public string Image12 { get; set; }
        public string ShippingLength { get; set; }
        public string ShippingHeight { get; set; }
        public string ShippingWidth { get; set; }
        public string ShippingWeight { get; set; }
        public string Qty { get; set; }
    }
}
