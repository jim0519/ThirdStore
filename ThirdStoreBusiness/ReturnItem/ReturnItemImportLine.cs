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


namespace ThirdStoreBusiness.ReturnItem
{
    public class ReturnItemImportLine
    {
        public string Record_ID { get; set; }
        public string Received_Date { get; set; }
        public string Location { get; set; }
        public string Supplier { get; set; }
        public string Tracking { get; set; }
        public string Return_Type { get; set; }
        public string NOP { get; set; }
        public string Detected_SKU { get; set; }
        public string Comment { get; set; }
        public string Process_Date { get; set; }
        public string Status { get; set; }
        public string Created_By { get; set; }
        public string Duplicate_Check { get; set; }
      
    }
}
