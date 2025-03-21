using System.Web.Optimization;

namespace IT_Inventory
{
    public class BundleConfig
    {

        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));


            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            bundles.Add(new ScriptBundle("~/bundles/adminlte").Include(
    "~/Content/Theme/AdminLTE/plugins/jquery/jquery.min.js",
    "~/Content/Theme/AdminLTE/plugins/bootstrap/js/bootstrap.bundle.min.js",
    "~/Content/Theme/AdminLTE/plugins/overlayScrollbars/js/jquery.overlayScrollbars.min.js",
    "~/Content/Theme/AdminLTE/dist/js/adminlte.min.js"
    ));

            bundles.Add(new StyleBundle("~/Content/adminlte/css").Include(
    "~/Content/Theme/AdminLTE/dist/css/adminlte.min.css",
    "~/Content/Theme/AdminLTE/plugins/fontawesome-free/css/all.min.css",
    "~/Content/Theme/AdminLTE/plugins/overlayScrollbars/css/OverlayScrollbars.min.css",
    "~/Content/Theme/AdminLTE/dist/css/adminlte.components.css",
    "~/Content/Theme/AdminLTE/dist/css/adminlte.components.css.map",
    "~/Content/Theme/AdminLTE/dist/css/adminlte.core.css",
    "~/Content/Theme/AdminLTE/dist/css/adminlte.core.css.map",
    "~/Content/Theme/AdminLTE/dist/css/adminlte.extra - components.css",
    "~/Content/Theme/AdminLTE/dist/css/adminlte.extra - components.css.map",
    "~/Content/Theme/AdminLTE/dist/css/adminlte.extra - components.min.map",
     "~/Content/Theme/AdminLTE/dist/css/adminlte.extra - components.min.css.map",
      "~/Content/Theme/AdminLTE/dist/css/adminlte.light.css",
      "~/Content/Theme/AdminLTE/dist/css/adminlte.light.css.map",
      "~/Content/Theme/AdminLTE/dist/css/adminlte.light.min.css",
        "~/Content/Theme/AdminLTE/dist/css/adminlte.light.min.css.map",
         "~/Content/Theme/AdminLTE/dist/css/adminlte.pages.css",
      "~/Content/Theme/AdminLTE/dist/css/adminlte.pages.css.map",
      "~/Content/Theme/AdminLTE/dist/css/adminlte.pages.min.css",
        "~/Content/Theme/AdminLTE/dist/css/adminlte.pages.min.css.map",
         "~/Content/Theme/AdminLTE/dist/css/adminlte.plugins.css",
      "~/Content/Theme/AdminLTE/dist/css/adminlte.plugins.css.map",
      "~/Content/Theme/AdminLTE/dist/css/adminlte.plugins.min.css",
        "~/Content/Theme/AdminLTE/dist/css/adminlte.plugins.min.css.map"

   ));

        }
    }
}
