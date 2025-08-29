using DBIT_Inventory.Services;
using IT_Inventory.Models;
using Owin;

namespace DBIT_Inventory
{
    public class Startup
    {

        public void Configuration(IAppBuilder app)
        {
            using (var context = new DBInventory())
            {
                var userService = new UserService(context);
                userService.InitializeAdmin();
            }
        }
    }
}
