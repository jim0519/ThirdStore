using FluentValidation;
using ThirdStore.Models.Item;
using ThirdStore.Framework.Validators;
using ThirdStoreCommon;
using ThirdStoreBusiness.Item;

namespace ThirdStore.Validators.Item
{
    public class ItemViewValidator : BaseValidator<ItemViewModel>
    {
        //private readonly IItemService _itemService;
        public ItemViewValidator(IItemService itemService)
        {
            //_itemService = itemService;

            RuleFor(x => x.SKU)
                .NotEmpty()
                .WithMessage("SKU cannot be empty")
                .Must((item, sku) => (item.ID == 0 && !itemService.IsDuplicateSKU(sku))
                || (item.ID > 0 && (itemService.GetItemByID(item.ID).SKU.ToLower().Equals(sku.ToLower()) || !itemService.IsDuplicateSKU(sku))))
                .WithMessage("SKU must not be duplicated")
                .Must((item, sku) => (item.ID > 0 && itemService.GetItemByID(item.ID).SKU.ToLower().Equals(sku.ToLower()))
                || (item.ID == 0 && !itemService.IsDuplicateSKU(sku)))
                .WithMessage("SKU cannot be changed");
            //.Must((item, sku) =>);

            RuleFor(x => x.Type)
                .Must((item, type) => {
                    if (type.Equals(ThirdStoreItemType.PART.ToValue()) && item.ChildItemLines.Count > 0
                    || type.Equals(ThirdStoreItemType.COMBO.ToValue()) && item.ChildItemLines.Count == 0)
                        return false;
                    else
                        return true;
                })
                .WithMessage("Combo should have child items / part should not have child items.");

        }
    }
}