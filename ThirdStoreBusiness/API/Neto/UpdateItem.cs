using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using ThirdStoreCommon.Models;

namespace ThirdStoreBusiness.API.Neto
{
    public class UpdateItem 
    {
        [System.Xml.Serialization.XmlElementAttribute("Item")]
        public UpdateItemItem[] Item { get; set; }
    }

    public class UpdateItemItem
    {
        public string SKU { get; set; }
        #region Name
        public string Name { get; set; }
        public bool ShouldSerializeName()
        {
            return !string.IsNullOrEmpty(Name);
        }
        #endregion

        #region DefaultPrice
        public decimal? DefaultPrice { get; set; }
        public bool ShouldSerializeDefaultPrice()
        {
            return DefaultPrice.HasValue;
        }
        #endregion

        public UpdateItemItemImages Images { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("WarehouseQuantity")]
        public UpdateItemItemWarehouseQuantity[] WarehouseQuantity { get; set; }

        #region IsActive
        public bool? IsActive { get; set; }
        public bool ShouldSerializeIsActive()
        {
            return IsActive.HasValue;
        }
        #endregion

        #region Description

        [XmlIgnore]
        public string Description { get; set; }
        [XmlElement("Description")]
        public System.Xml.XmlCDataSection DescriptionCDATA
        {
            get
            {
                return new System.Xml.XmlDocument().CreateCDataSection(Description);
            }
            set
            {
                Description = value.Value;
            }
        }

        public bool ShouldSerializeDescriptionCDATA()
        {
            return !string.IsNullOrEmpty(Description);
        }

        #endregion

        #region ShippingHeight
        public decimal? ShippingHeight { get; set; }
        public bool ShouldSerializeShippingHeight()
        {
            return ShippingHeight.HasValue;
        }
        #endregion

        #region ShippingLength
        public decimal? ShippingLength { get; set; }
        public bool ShouldSerializeShippingLength()
        {
            return ShippingLength.HasValue;
        }
        #endregion

        #region ShippingWidth
        public decimal? ShippingWidth { get; set; }
        public bool ShouldSerializeShippingWidth()
        {
            return ShippingWidth.HasValue;
        }
        #endregion

        #region ShippingWeight
        public decimal? ShippingWeight { get; set; }
        public bool ShouldSerializeShippingWeight()
        {
            return ShippingWeight.HasValue;
        }
        #endregion
    }

    public class UpdateItemItemWarehouseQuantity
    {
        public string WarehouseID { get; set; }
        public string Quantity { get; set; }
        public UpdateItemItemWarehouseQuantityAction Action { get; set; }
    }

    public enum UpdateItemItemWarehouseQuantityAction
    {

        /// <remarks/>
        increment,

        /// <remarks/>
        decrement,

        /// <remarks/>
        set,
    }

    public class UpdateItemItemImages
    {
        [System.Xml.Serialization.XmlElementAttribute("Image")]
        public UpdateItemItemImage[] Image { get; set; }
    }

    public class UpdateItemItemImage
    {
        public string Name { get; set; }
        public string URL { get; set; }
        public bool Delete { get; set; }
    }
}
