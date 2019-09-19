using System.Web;
using System.Web.Optimization;

namespace ThirdStore
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            BundleTable.EnableOptimizations = false;
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include("~/Scripts/jquery-ui.min.js", "~/Scripts/jquery-migrate-3.0.0.min.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/jquery-ui.min.css",
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            bundles.Add(new ScriptBundle("~/bundles/kendojs").Include("~/Content/kendo/js/kendo.all.min.js", "~/Scripts/Common.js"));
            //bundles.Add(new StyleBundle("~/bundles/kendocss").Include("~/Content/kendo/css/kendo.common.min.css", new CssRewriteUrlTransform()).Include(
            //    "~/Content/kendo/css/kendo.default.min.css", new CssRewriteUrlTransform()).Include(
            //    "~/Content/kendo/css/kendo.rtl.min.css", new CssRewriteUrlTransform()));

            bundles.Add(new StyleBundle("~/bundles/kendocss").Include("~/Content/kendo/css/kendo.common.min.css",
                "~/Content/kendo/css/kendo.default.min.css",
                "~/Content/kendo/css/kendo.rtl.min.css"));
        }
    }
}
