using FluentValidation;
using ThirdStore.Models.JobItem;
using ThirdStore.Framework.Validators;
using ThirdStoreCommon;
using ThirdStoreBusiness.JobItem;
using ThirdStoreBusiness.Item;
using FluentValidation.Validators;
using FluentValidation.Mvc;
using System.Web.ModelBinding;
using System.Web.Mvc;
using FluentValidation.Internal;
using System.Collections.Generic;

namespace ThirdStore.Validators.JobItem
{
    public class JobItemViewValidator : BaseValidator<JobItemViewModel>
    {
        //private readonly IItemService _itemService;
        public JobItemViewValidator(IJobItemService jobItemService,
            IItemService itemService)
        {

            //RuleFor(x => x.DesignatedSKU)
            //    .SetValidator(new DesignatedSKUExistValidator(jobItemService, itemService));
                //.SetValidator(new ItemLineValidator(jobItemService, itemService));
                

        }
    }

    public class DesignatedSKUExistValidator : PropertyValidator
    {
        private readonly IJobItemService _jobItemService;
        private readonly IItemService _itemService;

        public DesignatedSKUExistValidator(IJobItemService jobItemService,
            IItemService itemService) :base("Designated SKU does not exists")
        {
            _jobItemService = jobItemService;
            _itemService = itemService;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var designatedSKU = context.PropertyValue as string;
            return string.IsNullOrEmpty( designatedSKU) ||_itemService.GetItemBySKU(designatedSKU)!=null;
        }
    }

    public class ItemLineValidator : PropertyValidator
    {
        private readonly IJobItemService _jobItemService;
        private readonly IItemService _itemService;

        public ItemLineValidator(IJobItemService jobItemService,
            IItemService itemService) : base("Item line not match designated SKU structure.")
        {
            _jobItemService = jobItemService;
            _itemService = itemService;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var validateJobItem = context.Instance as JobItemViewModel;
            var designatedSKU = context.PropertyValue as string;
            if (designatedSKU == null)
                return true;
            else
                return false;
        }
    }


    public class DesignatedSKUExistPropertyValidator : FluentValidationPropertyValidator
    {
        public DesignatedSKUExistPropertyValidator(System.Web.Mvc.ModelMetadata metadata, ControllerContext controllerContext, PropertyRule rule, IPropertyValidator validator)
            : base(metadata, controllerContext, rule, validator)
        {
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            if (!this.ShouldGenerateClientSideRules())
                yield break;

            var formatter = new MessageFormatter().AppendPropertyName(Rule.PropertyName);
            string message = formatter.BuildMessage(Validator.ErrorMessageSource.GetString(null));

            var rule = new ModelClientValidationRule
            {
                ValidationType = "remote",
                ErrorMessage = message
            };
            rule.ValidationParameters.Add("url", "/api/validation/isskuexists");

            yield return rule;
        }

    }
}