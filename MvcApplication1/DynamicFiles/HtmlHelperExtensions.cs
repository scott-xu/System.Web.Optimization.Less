using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace MvcApplication1.DynamicFiles
{
    public static class HtmlHelperExtensions
    {
        /// <summary>
        ///     Returns an array of <see cref="SelectListItem" /> for Bootstrap theme chooser.
        /// </summary>
        public static ICollection<SelectListItem> GetBootstrapThemeItems(this HtmlHelper htmlHelper)
        {
            return DynamicFileVirtualPathProvider.ThemeNames
                .Select(name => new SelectListItem
                {
                    Selected = name.Equals(DynamicFileVirtualPathProvider.CurrentThemeName),
                    Text = name,
                    Value = name
                })
                .ToArray();
        }

        /// <summary>
        ///     Returns an array of <see cref="SelectListItem" /> for culture chooser.
        /// </summary>
        public static ICollection<SelectListItem> GetCultureItems(this HtmlHelper htmlHelper)
        {
            var cultureNames = new[]
            {
                "en-US",
                "uk-UA",
                "cs-CZ",
            };

            return cultureNames
                .Select(CultureInfo.GetCultureInfo)
                .Select(culture => new SelectListItem
                {
                    Selected = culture.Equals(DynamicFileVirtualPathProvider.CurrentCulture),
                    Text = culture.NativeName,
                    Value = culture.Name
                })
                .ToArray();
        }
    }
}
