using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MvcApplication1.BootstrapTheme
{
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Returns an array of <see cref="SelectListItem"/> for Bootstrap theme chooser.
        /// </summary>
        public static ICollection<SelectListItem> GetBootstrapThemeItems(this HtmlHelper htmlHelper)
        {
            return BootstrapThemeVirtualPathProvider.ThemeNames
                .Select(name => new SelectListItem
                {
                    Selected = name.Equals(BootstrapThemeVirtualPathProvider.CurrentThemeName),
                    Text = name,
                    Value = name
                })
                .ToArray();
        }
    }
}