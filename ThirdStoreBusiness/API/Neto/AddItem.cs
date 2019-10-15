using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using ThirdStoreCommon.Models;

namespace ThirdStoreBusiness.API.Neto
{
    public class AddItem
    {
        [System.Xml.Serialization.XmlElementAttribute("Item")]
        public AddItemItem[] Item { get; set; }
    }

    public class AddItemItem
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

        #region Images
        public AddItemItemImages Images { get; set; }
        public bool ShouldSerializeImages()
        {
            return Images != null;
        }
        #endregion

        [System.Xml.Serialization.XmlElementAttribute("WarehouseQuantity")]
        public AddItemItemWarehouseQuantity[] WarehouseQuantity { get; set; }

        #region IsActive
        public bool? IsActive { get; set; }
        public bool ShouldSerializeIsActive()
        {
            return IsActive.HasValue;
        }
        #endregion

        #region Active
        public bool? Active { get; set; }
        public bool ShouldSerializeActive()
        {
            return Active.HasValue;
        }
        #endregion

        #region Visible
        public bool? Visible { get; set; }
        public bool ShouldSerializeVisible()
        {
            return Visible.HasValue;
        }
        #endregion

        #region TaxInclusive
        public bool? TaxInclusive { get; set; }
        public bool ShouldSerializeTaxInclusive()
        {
            return TaxInclusive.HasValue;
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

        public string PrimarySupplier { get; set; }

        public AddItemItemEBayItems eBayItems { get; set; }
    }

    public class AddItemItemEBayItems
    {
        [System.Xml.Serialization.XmlElementAttribute("eBayItem")]
        public AddItemItemEBayItem[] eBayItem { get; set; }
    }

    public class AddItemItemEBayItem
    {
        /// <summary>
        /// 1: New Never Used | Free postage within Australia (#1 3rd-stock [AU]
        /// 8: Used | Free postage within Australia (#8 3rd-stock [AU]
        /// </summary>
        public string ListingTemplateID { get; set; }
    }

    public class AddItemItemWarehouseQuantity
    {
        public string WarehouseID { get; set; }
        public string Quantity { get; set; }
        public UpdateItemItemWarehouseQuantityAction Action { get; set; }
    }

    public enum AddItemItemWarehouseQuantityAction
    {

        /// <remarks/>
        increment,

        /// <remarks/>
        decrement,

        /// <remarks/>
        set,
    }

    public class AddItemItemImages
    {
        [System.Xml.Serialization.XmlElementAttribute("Image")]
        public AddItemItemImage[] Image { get; set; }
    }

    public class AddItemItemImage
    {
        public string Name { get; set; }
        public string URL { get; set; }
        public bool Delete { get; set; }
    }
}
