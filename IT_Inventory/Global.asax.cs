using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using IT_Inventory.App_Start;
using IT_Inventory.Services;


namespace IT_Inventory
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {

            using (var context = new IT_Inventory())
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
