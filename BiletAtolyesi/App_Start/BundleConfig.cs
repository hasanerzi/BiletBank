using System.Web;
using System.Web.Optimization;

namespace BiletAtolyesi
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

 
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                         "~/Theme/assets/js/jquery.v2.0.3.js"
                        ));


            bundles.Add(new ScriptBundle("~/bundles/scripts").Include(
                      "~/Theme/assets/js/js-index3.js",
                      "~/Theme/assets/js/functions.js",
                      "~/Theme/assets/js/jquery-ui.js",
                      "~/Theme/assets/js/jquery.easing.js",
                      "~/Theme/rs-plugin/js/jquery.themepunch.revolution.min.js",
                      "~/Theme/assets/js/jquery.nicescroll.min.js",
                      "~/Theme/assets/js/jquery.nicescroll.min.js",
                      "~/Theme/assets/js/jquery.carouFredSel-6.2.1-packed.js",
                      "~/Theme/assets/js/helper-plugins/jquery.touchSwipe.min.js",
                      "~/Theme/assets/js/helper-plugins/jquery.mousewheel.min.js",
                      "~/Theme/assets/js/helper-plugins/jquery.transit.min.js",
                      "~/Theme/assets/js/helper-plugins/jquery.ba-throttle-debounce.min.js",
                      "~/Theme/assets/js/jquery.customSelect.js",
                      "~/Theme/dist/js/bootstrap.min.js"
            ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Theme/dist/css/bootstrap.css",
                      "~/Theme/assets/css/custom.css",
                      "~/Theme/examples/carousel/carousel.css",
                      "~/Theme/assets/css/font-awesome.css",
                      "~/Theme/assets/css/font-awesome-ie7.css",
                      "~/Theme/css/fullscreen.css",
                      "~/Theme/rs-plugin/css/settings.css",
                      "~/Theme/assets/css/jquery-ui.css"
                      ));
        }
    }
}
