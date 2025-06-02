using DBIT_Inventory.Services;
using Owin;

namespace DBIT_Inventory
{
    public class Startup
    {

        public void Configuration(IAppBuilder app)
        {
            using (var context = new DBIT_Inventory())
            {
                var userService = new UserService(context);
                userService.InitializeAdmin();
            }
        }
    }
}
