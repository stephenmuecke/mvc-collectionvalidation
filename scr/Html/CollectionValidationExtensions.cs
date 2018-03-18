using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using Sandtrap.Web.Validation;

namespace Sandtrap.Web.Html
{

    /// <summary>
    /// 
    /// </summary>
    public static class CollectionValidationExtensions
    {

        #region .Methods 

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static MvcHtmlString CollectionValidationMessageFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            return CollectionValidationMessageHelper(
                htmlHelper,
                ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData),
                ExpressionHelper.GetExpressionText(expression),
                new RouteValueDictionary(),
                null);
        }

        // TODO: Add other overloads (htmlAttributes, tag)

        #endregion

        #region .Helper methods 

        private static MvcHtmlString CollectionValidationMessageHelper(this HtmlHelper htmlHelper, ModelMetadata modelMetadata, string expression, IDictionary<string, object> htmlAttributes, string tag)
        {
            bool isClientSideValidationEnabled = htmlHelper.ViewContext.ClientValidationEnabled;
            string propertyName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(expression);
            if (!htmlHelper.ViewData.ModelState.ContainsKey(propertyName) && !isClientSideValidationEnabled)
            {
                return null;
            }
            ModelState modelState = htmlHelper.ViewData.ModelState[propertyName];
            ModelErrorCollection modelErrors = (modelState == null) ? null : modelState.Errors;
            ModelError modelError = (((modelErrors == null) || (modelErrors.Count == 0)) ? null : modelErrors.FirstOrDefault(m => !String.IsNullOrEmpty(m.ErrorMessage)) ?? modelErrors[0]);
            if (modelError == null && !isClientSideValidationEnabled)
            {
                return null;
            }
            if (String.IsNullOrEmpty(tag))
            {
                tag = "span";
            }
            TagBuilder html = new TagBuilder(tag);
            html.MergeAttributes(htmlAttributes);
            html.AddCssClass((modelError != null) ? HtmlHelper.ValidationMessageCssClassName : HtmlHelper.ValidationMessageValidCssClassName);
            if (modelError != null)
            {
                html.InnerHtml = modelError.ErrorMessage;
            }
            if (isClientSideValidationEnabled)
            {
                
                Dictionary<string, object> validationAttributes = GetValidationAttributes(modelMetadata);
                html.MergeAttributes(validationAttributes);
                html.MergeAttribute("data-col-message-for", propertyName);
            }
            return MvcHtmlString.Create(html.ToString());
        }

        private static Dictionary<string, object> GetValidationAttributes(ModelMetadata metadata)
        {
            Type type = metadata.ContainerType;
            PropertyInfo property = type.GetProperty(metadata.PropertyName);
            object[] customAttributes = property.GetCustomAttributes(typeof(CollectionValidationAttribute), true);
            Dictionary<string, object> htmlAttributes = new Dictionary<string, object>();
            foreach (CollectionValidationAttribute attribute in customAttributes)
            {
                foreach (KeyValuePair<string, object> item in attribute.GetHtmlDataAttrbutes(metadata.GetDisplayName()))
                {
                    htmlAttributes.Add(item.Key, item.Value);
                }
            }
            return htmlAttributes;
        }

        #endregion

    }

}
