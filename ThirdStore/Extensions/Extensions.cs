using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Web.Mvc;

namespace ThirdStore.Extensions
{


    public static class Extensions
    {
        public static string FieldIdFor<T, TResult>(this HtmlHelper<T> html, Expression<Func<T, TResult>> expression)
        {
            return html.ViewData.TemplateInfo.GetFullHtmlFieldId(ExpressionHelper.GetExpressionText(expression)).Replace('[', '_').Replace(']', '_');
        }

        public static string FieldNameFor<T, TResult>(this HtmlHelper<T> html, Expression<Func<T, TResult>> expression)
        {
            return html.ViewData.TemplateInfo.GetFullHtmlFieldName(ExpressionHelper.GetExpressionText(expression));
        }

        public static SelectList ToSelectList<TEnum>(this TEnum enumObj, bool markCurrentAsSelected = true, int[] valuesToExclude = null) where TEnum : struct
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException("An Enumeration type is required.", "enumObj");
            }
            var items = from enumValue in Enum.GetValues(typeof(TEnum)).Cast<TEnum>()
                        where (valuesToExclude == null) || !valuesToExclude.Contains<int>(Convert.ToInt32(enumValue))
                        select new { ID = Convert.ToInt32(enumValue), Name = Enum.GetName(typeof(TEnum), enumValue) };
            object selectedValue = null;
            if (markCurrentAsSelected)
            {
                selectedValue = Convert.ToInt32(enumObj);
            }
            return new SelectList(items, "ID", "Name", selectedValue);
        }

        public static SelectList ToSelectListByList<T>(this IEnumerable<T> list) where T:class
        {
            //if (!typeof(T).IsEnum)
            //{
            //    throw new ArgumentException("An Enumeration type is required.", "enumObj");
            //}
            if (typeof(T).GetProperty("ID") == null || typeof(T).GetProperty("Name") == null)
                return default(SelectList);
            
            return new SelectList(list, "ID", "Name", list.FirstOrDefault());
        }

        public static MvcHtmlString MakeNavBarActive(this UrlHelper helper, string controllerName, string actionName)
        {
            string result = "active";

            var currentControllerName = helper.RequestContext.RouteData.Values["controller"].ToString();
            var currentActionName = helper.RequestContext.RouteData.Values["action"].ToString();

            if (!currentControllerName.Equals(controllerName, StringComparison.OrdinalIgnoreCase) || !currentActionName.Equals(actionName, StringComparison.OrdinalIgnoreCase))
            {
                result = null;
            }

            return MvcHtmlString.Create(result);
        }

        public static MvcHtmlString MakeNavBarActive(this UrlHelper helper, string controllerName)
        {
            string result = "active";

            var currentControllerName = helper.RequestContext.RouteData.Values["controller"].ToString();

            if (!currentControllerName.Equals(controllerName, StringComparison.OrdinalIgnoreCase))
            {
                result = null;
            }

            return MvcHtmlString.Create(result);
        }

        public static bool IsDebug(this HtmlHelper htmlHelper)
        {
            #if DEBUG
                  return true;
            #else
                  return false;
            #endif
        }

        public static IEnumerable<TEnum> EnumToList<TEnum>(this HtmlHelper htmlHelper) where TEnum : struct
        {
            return (TEnum[])Enum.GetValues(typeof(TEnum));
        } 

    }
}
