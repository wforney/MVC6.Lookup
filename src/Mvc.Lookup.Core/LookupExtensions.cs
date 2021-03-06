﻿using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace NonFactors.Mvc.Lookup
{
    public static class LookupExtensions
    {
        public static IHtmlContent AutoComplete<TModel>(this IHtmlHelper<TModel> html,
            String name, MvcLookup model, Object value = null, Object htmlAttributes = null)
        {
            HtmlContentBuilder autocomplete = new HtmlContentBuilder();

            autocomplete.AppendHtml(FormAutoComplete(html, model, name, htmlAttributes));
            autocomplete.AppendHtml(FormHiddenInput(html, name, value));

            return autocomplete;
        }
        public static IHtmlContent AutoCompleteFor<TModel, TProperty>(this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TProperty>> expression, Object htmlAttributes = null)
        {
            return html.AutoCompleteFor(expression, GetModelFromExpression(expression), htmlAttributes);
        }
        public static IHtmlContent AutoCompleteFor<TModel, TProperty>(this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TProperty>> expression, MvcLookup model, Object htmlAttributes = null)
        {
            String name = ExpressionHelper.GetExpressionText(expression);
            HtmlContentBuilder autocomplete = new HtmlContentBuilder();

            autocomplete.AppendHtml(FormAutoComplete(html, model, name, htmlAttributes));
            autocomplete.AppendHtml(FormHiddenInputFor(html, expression));

            return autocomplete;
        }

        public static IHtmlContent Lookup<TModel>(this IHtmlHelper<TModel> html,
            String name, MvcLookup model, Object value = null, Object htmlAttributes = null)
        {
            TagBuilder lookup = new TagBuilder("div");

            lookup.InnerHtml.AppendHtml(html.AutoComplete(name, model, value, htmlAttributes));
            lookup.InnerHtml.AppendHtml(FormLookupBrowse());
            lookup.AddCssClass("mvc-lookup-group");

            return lookup;
        }
        public static IHtmlContent LookupFor<TModel, TProperty>(this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TProperty>> expression, Object htmlAttributes = null)
        {
            return html.LookupFor(expression, GetModelFromExpression(expression), htmlAttributes);
        }
        public static IHtmlContent LookupFor<TModel, TProperty>(this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TProperty>> expression, MvcLookup model, Object htmlAttributes = null)
        {
            TagBuilder inputGroup = new TagBuilder("div");
            inputGroup.AddCssClass("mvc-lookup-group");
            inputGroup.InnerHtml.AppendHtml(html.AutoCompleteFor(expression, model, htmlAttributes));
            inputGroup.InnerHtml.AppendHtml(FormLookupBrowse());

            return inputGroup;
        }

        private static MvcLookup GetModelFromExpression<TModel, TProperty>(Expression<Func<TModel, TProperty>> expression)
        {
            MemberExpression exp = expression.Body as MemberExpression;
            LookupAttribute lookup = exp.Member.GetCustomAttribute<LookupAttribute>();

            if (lookup == null)
                throw new LookupException($"'{exp.Member.Name}' property does not have a '{typeof(LookupAttribute).Name}' specified.");

            return (MvcLookup)Activator.CreateInstance(lookup.Type);
        }
        private static IHtmlContent FormAutoComplete(IHtmlHelper html, MvcLookup model, String hiddenInput, Object htmlAttributes)
        {
            IDictionary<String, Object> attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            if (attributes.ContainsKey("class"))
                attributes["class"] = $"{attributes["class"]} mvc-lookup-input".Trim();
            else
                attributes["class"] = "mvc-lookup-input";
            attributes["data-for"] = TagBuilder.CreateSanitizedId(hiddenInput, html.IdAttributeDotReplacement);
            attributes["data-filters"] = String.Join(",", model.AdditionalFilters);
            attributes["data-search"] = model.Filter.Search;
            attributes["data-order"] = model.Filter.Order;
            attributes["data-page"] = model.Filter.Page;
            attributes["data-rows"] = model.Filter.Rows;
            attributes["data-sort"] = model.Filter.Sort;
            attributes["data-title"] = model.Title;
            attributes["data-url"] = model.Url;

            return html.TextBox(hiddenInput + MvcLookup.Prefix, null, attributes);
        }

        private static IHtmlContent FormHiddenInputFor<TModel, TProperty>(IHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression)
        {
            IDictionary<String, Object> attributes = new Dictionary<String, Object>();
            attributes["class"] = "mvc-lookup-hidden-input";

            return html.HiddenFor(expression, attributes);
        }
        private static IHtmlContent FormHiddenInput(IHtmlHelper html, String name, Object value)
        {
            IDictionary<String, Object> attributes = new Dictionary<String, Object>();
            attributes["class"] = "mvc-lookup-hidden-input";

            return html.Hidden(name, value, attributes);
        }

        private static IHtmlContent FormLookupBrowse()
        {
            TagBuilder browse = new TagBuilder("span");
            browse.AddCssClass("mvc-lookup-browse");

            return browse;
        }
    }
}
