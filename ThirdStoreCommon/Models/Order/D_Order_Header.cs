using System;
using System.Collections.Generic;

namespace ThirdStoreCommon.Models.Order
{
    public partial class D_Order_Header : BaseEntity
    {
        public D_Order_Header()
        {
            OrderLines = new List<D_Order_Line>();
        }

        public int TypeID { get; set; }
        public int StatusID { get; set; }
        public System.DateTime OrderTime { get; set; }
        public string ChannelOrderID { get; set; }
        public string CustomerID { get; set; }
        public string ConsigneeName { get; set; }
        public string ShippingAddress1 { get; set; }
        public string ShippingAddress2 { get; set; }
        public string ShippingSuburb { get; set; }
        public string ShippingState { get; set; }
        public string ShippingPostcode { get; set; }
        public string ShippingCountry { get; set; }
        public string ConsigneeEmail { get; set; }
        public string ConsigneePhoneNo { get; set; }
        public string BillingName { get; set; }
        public string BillingAddress1 { get; set; }
        public string BillingAddress2 { get; set; }
        public string BillingSuburb { get; set; }
        public string BillingState { get; set; }
        public string BillingPostcode { get; set; }
        public string BillingCountry { get; set; }
        public string BillingEmail { get; set; }
        public string BillingPhoneNo { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Postage { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingMethod { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentTransactionID { get; set; }
        public Nullable<System.DateTime> PaidTime { get; set; }
        public string Carrier { get; set; }
        public string BuyerNote { get; set; }
        public string OrderNote { get; set; }
        public Nullable<System.DateTime> ShipmentTime { get; set; }
        public string Ref1 { get; set; }//netoOrderStatus
        public string Ref2 { get; set; }//Issue
        public string Ref3 { get; set; }//Action
        public string Ref4 { get; set; }//Note
        public string Ref5 { get; set; }//CS Note
        public System.DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime EditTime { get; set; }
        public string EditBy { get; set; }
        public virtual ICollection<D_Order_Line> OrderLines { get; set; }
    }
}
