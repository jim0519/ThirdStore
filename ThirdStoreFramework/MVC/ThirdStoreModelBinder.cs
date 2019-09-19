using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace ThirdStoreFramework.MVC
{
    public class ThirdStoreModelBinder : DefaultModelBinder
    {
        protected override void SetProperty(ControllerContext controllerContext,
      ModelBindingContext bindingContext,
      System.ComponentModel.PropertyDescriptor propertyDescriptor, object value)
        {
            if (propertyDescriptor.PropertyType == typeof(string))
            {
                var stringValue = (string)value;
                if (!string.IsNullOrWhiteSpace(stringValue))
                {
                    value = stringValue.Trim();
                }
                else
                {
                    value = null;
                }
            }

            base.SetProperty(controllerContext, bindingContext,
                                propertyDescriptor, value);
        }
    }
}
