using System.Linq;
using System.Linq.Expressions;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;

namespace LeaveManagement {
    public static class HtmlHelpersExtensions {

        private static IStringLocalizerFactory _LocalizerFactory;

        public static void RegisterLocalizer(IStringLocalizerFactory localizerFactory) {
            _LocalizerFactory = localizerFactory;
        }

        public static string DescriptionFor<TModel, TValue>(this IHtmlHelper<TModel> self, Expression<Func<TModel, TValue>> expression) {
            MemberExpression memberExpression = (MemberExpression)expression.Body;
            var displayAttribute = (DisplayAttribute)memberExpression.Member.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault();
            string description = displayAttribute?.Description ?? memberExpression.Member?.Name;
            description = (_LocalizerFactory?.Create(memberExpression.Expression.Type)[description]) ?? description;
            return description;
        }

        public static string ShortNameFor<TModel, TValue>(this IHtmlHelper<TModel> self, Expression<Func<TModel, TValue>> expression) {
            MemberExpression memberExpression = (MemberExpression)expression.Body;
            var displayAttribute = (DisplayAttribute)memberExpression.Member.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault();
            string shortName = displayAttribute?.ShortName ?? memberExpression.Member?.Name;
            shortName = (_LocalizerFactory?.Create(memberExpression.Expression.Type)[shortName]) ?? shortName;
            return shortName;
        }

        public static string PromptFor<TModel, TValue>(this IHtmlHelper<TModel> self, Expression<Func<TModel, TValue>> expression) {
            MemberExpression memberExpression = (MemberExpression)expression.Body;
            var displayAttribute = (DisplayAttribute)memberExpression.Member.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault();
            string prompt = displayAttribute?.Prompt ?? memberExpression.Member?.Name;
            prompt = (_LocalizerFactory?.Create(memberExpression.Expression.Type)[prompt]) ?? prompt;
            return prompt;
        }
    }

    
}