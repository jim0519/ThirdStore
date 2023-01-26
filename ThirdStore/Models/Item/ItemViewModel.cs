using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ThirdStoreFramework.MVC;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using FluentValidation.Attributes;
using ThirdStore.Validators.Item;

namespace ThirdStore.Models.Item
{
    [Validator(typeof(ItemViewValidator))]
    public class ItemViewModel : BaseEntityViewModel
    {
        public ItemViewModel()
        {
            this.ChildItemLines = new List<ChildItemLineViewModel>();
        }

        public string SKU { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }
        [AllowHtml]
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public decimal Price { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.0000}", ApplyFormatInEditMode = true)]
        public decimal GrossWeight { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.0000}", ApplyFormatInEditMode = true)]
        public decimal NetWeight { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.0000}", ApplyFormatInEditMode = true)]
        public decimal CubicWeight { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.0000}", ApplyFormatInEditMode = true)]
        public decimal Length { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.0000}", ApplyFormatInEditMode = true)]
        public decimal Width { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.0000}", ApplyFormatInEditMode = true)]
        public decimal Height { get; set; }
        [Display(Name ="Supplier")]
        public int SupplierID { get; set; }
        public int IsReadyForList { get; set; }
        public int IsActive { get; set; }
        [Display(Name = "Reference SKU")]
        public string Ref1 { get; set; }
        [Display(Name = "Neto Alias SKU")]
        public string Ref2 { get; set; }
        [Display(Name = "UPC")]
        public string Ref3 { get; set; }
        [Display(Name = "eBay Site")]
        public string Ref4 { get; set; }
        [Display(Name = "Notes")]
        public string Ref6 { get; set; }
        public int DisableDropship { get; set; }

        public bool ShowSyncInventory { get; set; }

        public IList<SelectListItem> ItemTypes { get; set; }
        public IList<SelectListItem> YesOrNo { get; set; }
        public IList<SelectListItem> Suppliers { get; set; }
        public IList<ChildItemLineViewModel> ChildItemLines { get; set; }
        public IList<ItemImageViewModel> ItemViewImages { get; set; }

        public IList<ItemAttachmentViewModel> ItemViewAttachments { get; set; }
        public string Notes { get; set; }

        public class ChildItemLineViewModel : BaseEntityViewModel
        {
            public int ChildItemID { get; set; }
            public string ChildItemSKU { get; set; }
            public int ChildItemQty { get; set; }
        }

        public class ItemImageViewModel: BaseEntityViewModel
        {
            public int ImageID { get; set; }
            public string ImageURL { get; set; }
            public string ImageName { get; set; }
            public int DisplayOrder { get; set; }
            public bool StatusID { get; set; }
        }

        public class ItemAttachmentViewModel : BaseEntityViewModel
        {
            public int AttachmentID { get; set; }
            public string AttachmentURL { get; set; }
            public string AttachmentName { get; set; }
            public int DisplayOrder { get; set; }
            public string Notes { get; set; }
        }
    }
}