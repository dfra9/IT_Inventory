using System.Linq;
using System.Web.Mvc;
using IT_Inventory.Models;
using IT_Inventory.ViewModel;


namespace IT_Inventory.Controllers
{
    public class HomeController : Controller
    {
        private readonly IT_Inventory db = new IT_Inventory();

        [Authorize]
        public ActionResult Index()
        {
            var user = Session["User"] as Users;
            var viewModel = new AssetManagementViewModel
            {
                TotalAssets = db.Asset.Count(a => a.Is_Deleted != true),
                AvailableAssets = db.Asset.Count(a => a.Is_Deleted != true && a.Status == "Ready"),
                AssetInUse = db.Asset.Count(a => a.Is_Deleted != true && a.Status == "Borrowing"),
                AssetInService = db.Asset.Count(a => a.Is_Deleted != true && a.Status == "Service"),

                DashHistory = db.Asset
                    .Where(a => a.Is_Deleted != true)
                    .OrderByDescending(a => a.Transaction_Date)
                    .Take(10)
                    .ToList()

            };
            return View(viewModel);
        }


        public ActionResult SearchDashboardAssets(string search)
        {
            var assetData = db.Asset
                .Where(a => a.Is_Deleted != true);
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                assetData = assetData.Where(a => (a.No_asset != null && a.No_asset.ToLower().Contains(search)) ||
                    (a.Material_Group != null && a.Material_Group.ToLower().Contains(search)) ||
                    (a.Material_Description != null && a.Material_Description.ToLower().Contains(search)) ||
                    (a.Location != null && a.Location.ToLower().Contains(search)) ||
                   (a.Departement != null && a.Departement.ToLower().Contains(search)) ||
                    (a.Status != null && a.Status.ToLower().Contains(search))
                    );
            }
            var assets = assetData.OrderByDescending(a => a.Transaction_Date).Select(a => new
            {
                a.No_asset,
                a.Company_Code,
                a.Material_Group,
                a.Material_Description,
                a.Acquisition_Date,
                a.Departement,
                a.Location,
                a.Status
            }).Take(10).ToList();
            return Json(assets, JsonRequestBehavior.AllowGet);
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Login()
        {
            return View("~/Views/Auth/Login.cshtml", new LoginViewModel());
        }



    }
}