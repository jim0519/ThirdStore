using LINQtoCSV;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ThirdStoreCommon;
using ThirdStoreCommon.Models.Order;


namespace ThirdStoreBusiness.Order
{
    public class DSZImportLine
    {
        //[CsvColumn(Name = "tracking-number")]
        public string serial_number { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string suburb { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string postcode { get; set; }
        public string telephone { get; set; }
        public string sku { get; set; }
        public decimal price { get; set; }
        public decimal postage { get; set; }
        public int qty { get; set; }
        public string comment { get; set; }
       
    }

    public class TemuOrderLine
    {
        //[CsvColumn(Name = "\uFEFForder id")]
        [CsvColumn(Name = "order id")]
        public string OrderID { get; set; }

        [CsvColumn(Name = "order status")]
        public string OrderStatus { get; set; }

        [CsvColumn(Name = "Order item ID")]
        public string OrderItemId { get; set; }

        [CsvColumn(Name = "product name")]
        public string ProductName { get; set; }

        [CsvColumn(Name = "variation")]
        public string Variation { get; set; }

        [CsvColumn(Name = "contribution sku")]
        public string SKU { get; set; }

        [CsvColumn(Name = "sku id")]
        public string SKUId { get; set; }

        [CsvColumn(Name = "quantity purchased")]
        public int QuantityPurchased { get; set; }

        [CsvColumn(Name = "recipient name")]
        public string RecipientName { get; set; }

        [CsvColumn(Name = "recipient first name")]
        public string RecipientFirstName { get; set; }

        [CsvColumn(Name = "recipient last name")]
        public string RecipientLastName { get; set; }

        [CsvColumn(Name = "recipient phone number")]
        public string RecipientPhoneNumber { get; set; }

        [CsvColumn(Name = "ship address 1")]
        public string ShipAddress1 { get; set; }

        [CsvColumn(Name = "ship address 2")]
        public string ShipAddress2 { get; set; }

        [CsvColumn(Name = "ship address 3")]
        public string ShipAddress3 { get; set; }

        [CsvColumn(Name = "ship city")]
        public string ShipCity { get; set; }

        [CsvColumn(Name = "ship state")]
        public string ShipState { get; set; }

        [CsvColumn(Name = "ship postal code (Must be shipped to the following zip code.)")]
        public string ShipPostalCode { get; set; }

        [CsvColumn(Name = "ship country ")]
        public string ShipCountry { get; set; }

        [CsvColumn(Name = "purchase date")]
        public string PurchaseDate { get; set; }

        [CsvColumn(Name = "latest delivery time")]
        public string LatestDeliveryTime { get; set; }

        [CsvColumn(Name = "virtual email")]
        public string VirtualEmail { get; set; }

        [CsvColumn(Name = "base price total")]
        public string BasePriceTotal { get; set; }

        [CsvColumn(Name = "tracking number")]
        public string TrackingNumber { get; set; }

        [CsvColumn(Name = "carrier")]
        public string Carrier { get; set; }
    }

    public class NetoOrderLine
    {
        [CsvColumn(Name = "Order ID", FieldIndex = 1)]
        public string OrderId { get; set; }

        [CsvColumn(Name = "Purchase Order ID", FieldIndex = 2)]
        public string PurchaseOrderId { get; set; }

        [CsvColumn(Name = "Group Orderlines By", FieldIndex = 3)]
        public string GroupOrderlinesBy { get; set; }

        [CsvColumn(Name = "Order Status", FieldIndex = 4)]
        public string OrderStatus { get; set; }

        [CsvColumn(Name = "Approved", FieldIndex = 5)]
        public string Approved { get; set; }

        [CsvColumn(Name = "Username", FieldIndex = 6)]
        public string Username { get; set; }

        [CsvColumn(Name = "Email", FieldIndex = 7)]
        public string Email { get; set; }

        [CsvColumn(Name = "Ship First Name", FieldIndex = 8)]
        public string ShipFirstName { get; set; }

        [CsvColumn(Name = "Ship Last Name", FieldIndex = 9)]
        public string ShipLastName { get; set; }

        [CsvColumn(Name = "Ship Company", FieldIndex = 10)]
        public string ShipCompany { get; set; }

        [CsvColumn(Name = "Ship Address Line 1", FieldIndex = 11)]
        public string ShipAddressLine1 { get; set; }

        [CsvColumn(Name = "Ship Address Line 2", FieldIndex = 12)]
        public string ShipAddressLine2 { get; set; }

        [CsvColumn(Name = "Ship City", FieldIndex = 13)]
        public string ShipCity { get; set; }

        [CsvColumn(Name = "Ship State", FieldIndex = 14)]
        public string ShipState { get; set; }

        [CsvColumn(Name = "Ship Post Code", FieldIndex = 15)]
        public string ShipPostCode { get; set; }

        [CsvColumn(Name = "Ship Country", FieldIndex = 16)]
        public string ShipCountry { get; set; }

        [CsvColumn(Name = "Ship Phone", FieldIndex = 17)]
        public string ShipPhone { get; set; }

        [CsvColumn(Name = "Ship Fax", FieldIndex = 18)]
        public string ShipFax { get; set; }

        [CsvColumn(Name = "Bill First Name", FieldIndex = 19)]
        public string BillFirstName { get; set; }

        [CsvColumn(Name = "Bill Last Name", FieldIndex = 20)]
        public string BillLastName { get; set; }

        [CsvColumn(Name = "Bill Company", FieldIndex = 21)]
        public string BillCompany { get; set; }

        [CsvColumn(Name = "Bill Address Line 1", FieldIndex = 22)]
        public string BillAddressLine1 { get; set; }

        [CsvColumn(Name = "Bill Address Line 2", FieldIndex = 23)]
        public string BillAddressLine2 { get; set; }

        [CsvColumn(Name = "Bill City", FieldIndex = 24)]
        public string BillCity { get; set; }

        [CsvColumn(Name = "Bill State", FieldIndex = 25)]
        public string BillState { get; set; }

        [CsvColumn(Name = "Bill Post Code", FieldIndex = 26)]
        public string BillPostCode { get; set; }

        [CsvColumn(Name = "Bill Country", FieldIndex = 27)]
        public string BillCountry { get; set; }

        [CsvColumn(Name = "Bill Phone", FieldIndex = 28)]
        public string BillPhone { get; set; }

        [CsvColumn(Name = "Bill Fax", FieldIndex = 29)]
        public string BillFax { get; set; }

        [CsvColumn(Name = "Payment Method", FieldIndex = 30)]
        public string PaymentMethod { get; set; }

        [CsvColumn(Name = "Shipping Method", FieldIndex = 31)]
        public string ShippingMethod { get; set; }

        [CsvColumn(Name = "Shipping Cost", FieldIndex = 32)]
        public string ShippingCost { get; set; }

        [CsvColumn(Name = "Shipping Discount Amount", FieldIndex = 33)]
        public string ShippingDiscountAmount { get; set; }

        [CsvColumn(Name = "Customer Instructions", FieldIndex = 34)]
        public string CustomerInstructions { get; set; }

        [CsvColumn(Name = "Internal Notes", FieldIndex = 35)]
        public string InternalNotes { get; set; }

        [CsvColumn(Name = "Amount Paid", FieldIndex = 36)]
        public string AmountPaid { get; set; }

        [CsvColumn(Name = "Date Paid", FieldIndex = 37)]
        public string DatePaid { get; set; }

        [CsvColumn(Name = "Order Line SKU", FieldIndex = 38)]
        public string OrderLineSku { get; set; }

        [CsvColumn(Name = "Order Line Qty", FieldIndex = 39)]
        public int OrderLineQty { get; set; }

        [CsvColumn(Name = "Order Line Description", FieldIndex = 40)]
        public string OrderLineDescription { get; set; }

        [CsvColumn(Name = "Order Line Serial Number", FieldIndex = 41)]
        public string OrderLineSerialNumber { get; set; }

        [CsvColumn(Name = "Order Line Warehouse Name", FieldIndex = 42)]
        public string OrderLineWarehouseName { get; set; }

        [CsvColumn(Name = "Order Line Unit Price", FieldIndex = 43)]
        public string OrderLineUnitPrice { get; set; }

        [CsvColumn(Name = "Order Line Discount Amount", FieldIndex = 44)]
        public string OrderLineDiscountAmount { get; set; }

        [CsvColumn(Name = "Order Line Notes", FieldIndex = 45)]
        public string OrderLineNotes { get; set; }
    }

    public class ExportTemuOrderTrackingLine
    {
        [Display(Name = "order id")]
        [CsvColumn(Name = "order id", FieldIndex = 1)]
        public string OrderID { get; set; }
        [Display(Name = "order item id")]
        [CsvColumn(Name = "order item id", FieldIndex = 2)]
        public string OrderItemID { get; set; }
        [Display(Name = "quantity")]
        [CsvColumn(Name = "quantity", FieldIndex = 3)]
        public int Quantity { get; set; }
        [Display(Name = "Ship from")]
        [CsvColumn(Name = "Ship from", FieldIndex = 4)]
        public string ShipFrom { get; set; }
        [Display(Name = "carrier")]
        [CsvColumn(Name = "carrier", FieldIndex = 5)]
        public string Carrier { get; set; }
        [Display(Name = "tracking number")]
        [CsvColumn(Name = "tracking number", FieldIndex = 6)]
        public string TrackingNumber { get; set; }
    }
}
