using LINQtoCSV;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
}
