using LINQtoCSV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ThirdStoreBusiness.DSChannel
{
    public class DSZModel
    {
        public string SKU { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        [CsvColumn(Name = "Stock Qty")]
        public decimal InventoryQty { get; set; }
        public string Status { get; set; }
        [CsvColumn(Name = "price")]
        public decimal Price { get; set; }
        public string RrpPrice { get; set; }
        //public string VIC { get; set; }
        //public string NSW { get; set; }
        //public string SA { get; set; }
        //public string QLD { get; set; }
        //public string TAS { get; set; }
        //public string WA { get; set; }
        //public string NT { get; set; }
        public string ACT { get; set; }
        public string NSW_M { get; set; }
        public string NSW_R { get; set; }
        public string NT_M { get; set; }
        public string NT_R { get; set; }
        public string QLD_M { get; set; }
        public string QLD_R { get; set; }
        public string REMOTE { get; set; }
        public string SA_M { get; set; }
        public string SA_R { get; set; }
        public string TAS_M { get; set; }
        public string TAS_R { get; set; }
        public string VIC_M { get; set; }
        public string VIC_R { get; set; }
        public string WA_M { get; set; }
        public string WA_R { get; set; }
        [CsvColumn(Name = "bulky item")]
        public string IsBulkyItem { get; set; }
        public string Discontinued { get; set; }
        [CsvColumn(Name = "EAN Code")]
        public string EANCode { get; set; }
        public string Brand { get; set; }
        public string MPN { get; set; }
        [CsvColumn(Name = "Weight (kg)")]
        public string Weight { get; set; }
        [CsvColumn(Name = "Carton Length (cm)")]
        public string Length { get; set; }
        [CsvColumn(Name = "Carton Width (cm)")]
        public string Width { get; set; }
        [CsvColumn(Name = "Carton Height (cm)")]
        public string Height { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        [CsvColumn(Name = "Image 1")]
        public string Image1 { get; set; }
        [CsvColumn(Name = "Image 2")]
        public string Image2 { get; set; }
        [CsvColumn(Name = "Image 3")]
        public string Image3 { get; set; }
        [CsvColumn(Name = "Image 4")]
        public string Image4 { get; set; }
        [CsvColumn(Name = "Image 5")]
        public string Image5 { get; set; }
        [CsvColumn(Name = "Image 6")]
        public string Image6 { get; set; }
        [CsvColumn(Name = "Image 7")]
        public string Image7 { get; set; }
        [CsvColumn(Name = "Image 8")]
        public string Image8 { get; set; }
        [CsvColumn(Name = "Image 9")]
        public string Image9 { get; set; }
        [CsvColumn(Name = "Image 10")]
        public string Image10 { get; set; }
        [CsvColumn(Name = "Image 11")]
        public string Image11 { get; set; }
        [CsvColumn(Name = "Image 12")]
        public string Image12 { get; set; }
        [CsvColumn(Name = "Image 13")]
        public string Image13 { get; set; }
        [CsvColumn(Name = "Image 14")]
        public string Image14 { get; set; }
        [CsvColumn(Name = "Image 15")]
        public string Image15 { get; set; }





        //public string SKU { get; set; }
        //public string Title { get; set; }
        //public string Description { get; set; }
        ////public string ListingTitle { get; set; }
        ////public string ListingTitle { get; set; }
        //[CsvColumn(Name = "price")]
        //public decimal Price { get; set; }
        //[CsvColumn(Name = "Stock Qty")]
        //public int InventoryQty { get; set; }
        //public string Status { get; set; }
        //public string VIC { get; set; }
        //public string NSW { get; set; }
        //public string SA { get; set; }
        //public string QLD { get; set; }
        //public string TAS { get; set; }
        //public string WA { get; set; }
        //public string NT { get; set; }
        //[CsvColumn(Name = "bulky item")]
        //public string IsBulkyItem { get; set; }
        //[CsvColumn(Name = "Weight (kg)")]
        //public string Weight { get; set; }
        //[CsvColumn(Name = "Carton Length (cm)")]
        //public string Length { get; set; }
        //[CsvColumn(Name = "Carton Width (cm)")]
        //public string Width { get; set; }
        //[CsvColumn(Name = "Carton Height (cm)")]
        //public string Height { get; set; }
        ////[CsvColumn(Name = "images")]
        ////public string Images { get; set; }
        //public string RrpPrice { get; set; }
        //public string Category { get; set; }
        //public string Discontinued { get; set; }
        //[CsvColumn(Name = "EAN Code")]
        //public string EANCode { get; set; }
        //public string Brand { get; set; }
        //public string MPN { get; set; }

        //[CsvColumn(Name = "Image 1")]
        //public string Image1 { get; set; }
        //[CsvColumn(Name = "Image 2")]
        //public string Image2 { get; set; }
        //[CsvColumn(Name = "Image 3")]
        //public string Image3 { get; set; }
        //[CsvColumn(Name = "Image 4")]
        //public string Image4 { get; set; }
        //[CsvColumn(Name = "Image 5")]
        //public string Image5 { get; set; }
        //[CsvColumn(Name = "Image 6")]
        //public string Image6 { get; set; }
        //[CsvColumn(Name = "Image 7")]
        //public string Image7 { get; set; }
        //[CsvColumn(Name = "Image 8")]
        //public string Image8 { get; set; }
        //[CsvColumn(Name = "Image 9")]
        //public string Image9 { get; set; }
        //[CsvColumn(Name = "Image 10")]
        //public string Image10 { get; set; }
        //[CsvColumn(Name = "Image 11")]
        //public string Image11 { get; set; }
        //[CsvColumn(Name = "Image 12")]
        //public string Image12 { get; set; }
        //[CsvColumn(Name = "Image 13")]
        //public string Image13 { get; set; }
        //[CsvColumn(Name = "Image 14")]
        //public string Image14 { get; set; }
        //[CsvColumn(Name = "Image 15")]
        //public string Image15 { get; set; }
    }

    public class SelloDSModel
    {
        public string sku { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int qty { get; set; }
        public decimal price { get; set; }
        public decimal special_price { get; set; }
        public string length { get; set; }
        public string width { get; set; }
        public string height { get; set; }
        public string weight { get; set; }
        public string main_image { get; set; }
        public string additional_images { get; set; }
        [CsvColumn(Name = "Shipping Operation")]
        public string shipping_operation { get; set; }
        public string UPC { get; set; }
        public string Brand { get; set; }
    }

    public class DSZImportLine
    {
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

    public class SelloImportLine
    {
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
        public int qty { get; set; }
        public string comment { get; set; }
    }
}