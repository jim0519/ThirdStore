using LINQtoCSV;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdStoreCommon;
using ThirdStoreCommon.Models.JobItem;


namespace ThirdStoreBusiness.JobItem
{
    public class GumtreeFeed
    {
        [CsvColumn(FieldIndex = 1)]
        public string JobItemIDs { get; set; }
        [CsvColumn(FieldIndex = 2)]
        public string References { get; set; }
        [CsvColumn(FieldIndex = 3)]
        public DateTime CreateTime { get; set; }
        [CsvColumn(FieldIndex = 4)]
        public string Condition { get; set; }
        [CsvColumn(FieldIndex = 5)]
        public string SKU { get; set; }
        [CsvColumn(FieldIndex = 6)]
        public string Title { get; set; }
        [CsvColumn(FieldIndex = 7)]
        public decimal Price { get; set; }
        [CsvColumn(FieldIndex = 8)]
        public string Description { get; set; }
        [CsvColumn(FieldIndex = 9)]
        public string Image1 { get; set; }
        [CsvColumn(FieldIndex = 10)]
        public string Image2 { get; set; }
        [CsvColumn(FieldIndex = 11)]
        public string Image3 { get; set; }
        [CsvColumn(FieldIndex = 12)]
        public string Image4 { get; set; }
        [CsvColumn(FieldIndex = 13)]
        public string Image5 { get; set; }
        [CsvColumn(FieldIndex = 14)]
        public string Image6 { get; set; }
        [CsvColumn(FieldIndex = 15)]
        public string Image7 { get; set; }
        [CsvColumn(FieldIndex = 16)]
        public string Image8 { get; set; }
        [CsvColumn(FieldIndex = 17)]
        public string Image9 { get; set; }
        [CsvColumn(FieldIndex = 18)]
        public string Image10 { get; set; }
        [CsvColumn(FieldIndex = 19)]
        public string Image11 { get; set; }
        [CsvColumn(FieldIndex = 20)]
        public string Image12 { get; set; }
        [CsvColumn(FieldIndex = 21)]
        public int ID { get; set; }
    }
}
