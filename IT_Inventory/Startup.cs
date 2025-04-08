using IT_Inventory.Services;
using Owin;

namespace IT_Inventory
{
    public class Startup
    {

        public void Configuration(IAppBuilder app)
        {
            using (var context = new IT_Inventory())
            {
                var userService = new UserService(context);
                userService.InitializeAdmin();
            }
        }
    }
}
