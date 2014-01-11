using System.Web.Optimization;
using MvcApplication1.DynamicFiles;

namespace MvcApplication1
{
    public class BundleConfig
    {
        public const string JQUERY_PATH = "~/bundles/jquery";
        public const string JQUERY_GLOBALIZE_PATH = "~/bundles/jquery-globalize";
        public const string BOOTSTRAP_SCRIPTS_PATH = "~/bundles/bootstrap";
        public const string BOOTSTRAP_STYLES_PATH = "~/Content/bootstrap";

        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle(JQUERY_PATH).Include("~/Scripts/jquery-{version}.js"));

            // script bundle with transient file content:
            // appropriate globalize.culture.{culture-name}.js file is resolved in BootstrapThemeVirtualPathProvider
            bundles.Add(new DynamicScriptBundle(JQUERY_GLOBALIZE_PATH).Include(
                "~/Scripts/jquery.globalize/globalize.js",
                // the following are dynamic
                DynamicFileVirtualPathProvider.GLOBALIZE_CULTURE_PATH,
                DynamicFileVirtualPathProvider.GLOBALIZE_RESET_PATH));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"));

            bundles.Add(new LessBundle("~/bundles/files-from-different-folders-with-imports")
                .Include("~/content/less-2/layouts.less")
                .Include("~/content/less/media.less")
                .Include("~/content/layouts.less"));

            // Add @Styles.Render("~/Content/bootstrap") in the <head/> of your _Layout.cshtml view
            // Add @Scripts.Render("~/bundles/bootstrap") after jQuery in your _Layout.cshtml view
            // When <compilation debug="true" />, MVC4 will render the full readable version. When set to <compilation debug="false" />, the minified version will be rendered automatically
            bundles.Add(new ScriptBundle(BOOTSTRAP_SCRIPTS_PATH).Include(
                "~/Scripts/bootstrap.js",
                "~/Scripts/bootstrap-datepicker.js",
                "~/Scripts/bootstrap-datepicker-globalize.js"));

            bundles.Add(new LessBundle(BOOTSTRAP_STYLES_PATH).Include(
                "~/Content/less/bootstrap.less",
                //"~/Content/less/responsive.less",
                "~/Content/bootstrap-datepicker.css"));
        }
    }
}
