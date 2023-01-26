using System;
using System.Collections.Generic;
using ThirdStoreCommon.Models.Attachment;
using ThirdStoreCommon.Models.Image;

namespace ThirdStoreCommon.Models.Item
{
    public partial class D_Item:BaseEntity
    {
        public D_Item()
        {
            this.ParentItems = new List<D_Item_Relationship>();
            this.ChildItems = new List<D_Item_Relationship>();
            this.ItemImages = new List<M_ItemImage>();
            //this.SubstituteItemGroupLines = new List<D_Substitute_Item_Group_Line>();
        }

        public string SKU { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public decimal Price { get; set; }
        public decimal GrossWeight { get; set; }
        public decimal NetWeight { get; set; }
        public decimal CubicWeight { get; set; }
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public int SupplierID { get; set; }
        public bool IsReadyForList { get; set; }
        public bool IsActive { get; set; }
        public string Ref1 { get; set; }//Reference SKU
        public string Ref2 { get; set; }//Neto Alias SKU
        public string Ref3 { get; set; }//UPC
        public string Ref4 { get; set; }//Link
        public string Ref5 { get; set; }//For supplier P and S, Image URLs; For supplier A, Categories
        public string Ref6 { get; set; }//Notes
        public bool DisableDropship { get; set; }

        public System.DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime EditTime { get; set; }
        public string EditBy { get; set; }

        public virtual ICollection<D_Item_Relationship> ParentItems { get; set; }
        public virtual ICollection<D_Item_Relationship> ChildItems { get; set; }
        public virtual ICollection<M_ItemImage> ItemImages { get; set; }
        public virtual ICollection<M_ItemAttachment> ItemAttachments { get; set; }
    }
}
