using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using DBIT_Inventory.App_Start;
using DBIT_Inventory.Services;


namespace DBIT_Inventory
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {

            using (var context = new DBIT_Inventory())
            {
                var userService = new UserService(context);
                userService.InitializeAdmin();
            }

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            DependencyConfig.RegisterDependencies();


        }
    }
}
