using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ThirdStoreFramework.MVC;

namespace ThirdStore.Models.Item
{
    public class ItemGridViewModel : BaseEntityViewModel
    {
        public string SKU { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Supplier { get; set; }
        public bool IgnoreListing { get; set; }
        public decimal Price { get; set; }
    }
}