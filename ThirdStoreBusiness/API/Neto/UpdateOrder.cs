using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using ThirdStoreCommon.Models;

namespace ThirdStoreBusiness.API.Neto
{
    public class UpdateOrder
    {
        [System.Xml.Serialization.XmlElementAttribute("Order")]
        public UpdateOrderOrder[] Order { get; set; }
    }

    public class UpdateOrderOrder
    {
        public string OrderID { get; set; }
        #region ShipInstructions
        public string ShipInstructions { get; set; }
        #endregion

    }

}
