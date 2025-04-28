using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using IT_Inventory.Models;
using IT_Inventory.ViewModel;
using static IT_Inventory.ViewModel.AssetManagementViewModel;

namespace IT_Inventory.Controllers
{
    public class AssetManagementController : Controller
    {
        private readonly IT_Inventory db = new IT_Inventory();
        // GET: AssetManagement
        public ActionResult Index()
        {
            var viewModel = new AssetManagementViewModel
            {
                Companies = db.Company.Where(c => c.Is_Deleted != true).ToList(),
                Dept = db.Departement.Where(c => c.Is_Deleted != true).ToList(),
                Cities = db.City.Where(c => c.Is_Deleted != true).ToList(),
                MaterialGroup = db.Material_Group.Where(c => c.Is_Deleted != true).ToList(),
                Material_Code1 = db.Material_Code.Where(c => c.Is_Deleted != true).ToList(),
                UoMList = db.UoM.Where(c => c.Is_Deleted != true).ToList(),
                AssetHistory = db.Asset
                    .Where(a => a.Is_Deleted != true)
                    .GroupBy(a => a.No_asset)
                    .Select(g => g.OrderByDescending(a => a.Transaction_Date).FirstOrDefault())
                    .OrderByDescending(a => a.Transaction_Date)
                    .Take(100)
                    .ToList(),
                DashboardCounts = GetDashboardCounts()
            };

            var defaultCity = viewModel.Cities.FirstOrDefault()?.City_Name;
            viewModel.LocationsList = db.Location
                .Where(l => l.Is_Deleted != true && l.City_Name == defaultCity)
                .ToList();
            viewModel.Companies = viewModel.Companies ?? new List<Company>();
            viewModel.Dept = viewModel.Dept ?? new List<Departement>();
            viewModel.LocationsList = viewModel.LocationsList ?? new List<Location>();
            viewModel.Cities = viewModel.Cities ?? new List<City>();
            viewModel.MaterialGroup = viewModel.MaterialGroup ?? new List<Material_Group>();
            viewModel.Material_Code1 = viewModel.Material_Code1 ?? new List<Material_Code>();
            viewModel.UoMList = viewModel.UoMList ?? new List<UoM>();
            viewModel.AssetHistory = viewModel.AssetHistory ?? new List<Asset>();

            LoadDropdownData();
            return View(viewModel);
        }

        public ActionResult Editor(string id = null, string mode = "Create")
        {
            AssetManagementViewModel viewModel = new AssetManagementViewModel();
            viewModel.mode = mode;

            LoadDropdownData();


            viewModel.Companies = db.Company.Where(c => c.Is_Deleted != true).ToList();
            viewModel.Dept = db.Departement.Where(c => c.Is_Deleted != true).ToList();
            viewModel.Cities = db.City.Where(c => c.Is_Deleted != true).ToList();
            viewModel.MaterialGroup = db.Material_Group
            .Where(c => c.Is_Deleted != true || c.Is_Deleted == null)
            .ToList();
            viewModel.Material_Code1 = db.Material_Code.Where(c => c.Is_Deleted != true).ToList();
            viewModel.UoMList = db.UoM.Where(c => c.Is_Deleted != true).ToList();

            if (id != null && (mode == "Edit" || mode == "View" || mode == "Delete"))
            {
                var asset = db.Asset
                .Where(a => a.No_asset == id && a.Is_Deleted != true)
                .OrderByDescending(a => a.Transaction_Date)
                .FirstOrDefault();

                if (asset != null)
                {
                    viewModel.No_asset = asset.No_asset;
                    viewModel.Company_Code = asset.Company_Code;
                    viewModel.Company_Name = asset.Company_Name;
                    viewModel.Material_Group = asset.Material_Group;
                    viewModel.Material_Code = asset.Material_Code;
                    viewModel.Material_Description = asset.Material_Description;
                    viewModel.Quantity = asset.Quantity;
                    viewModel.UoM = asset.UoM;
                    viewModel.Serial_Number = asset.Serial_Number;
                    viewModel.Device_Id = asset.Device_Id;
                    viewModel.Acquisition_Date = asset.Acquisition_Date;
                    viewModel.Acquisition_value = asset.Acquisition_value;
                    viewModel.No_Asset_PGA = asset.No_Asset_PGA;
                    viewModel.No_Asset_Accounting = asset.No_Asset_Accounting;
                    viewModel.No_PO = asset.No_PO;
                    viewModel.Latest_User = asset.Latest_User;

                    viewModel.Departement_Code = db.Departement
                        .Where(d => d.Departement_Name == asset.Departement && d.Is_Deleted != true)
                        .Select(d => d.Departement_Code)
                        .FirstOrDefault();
                    viewModel.Departement_Name = asset.Departement;
                    viewModel.City_Name = asset.City;
                    var location = db.Location.FirstOrDefault(l =>
                l.Location_Name == asset.Location &&
                l.City_Name == asset.City &&
                l.Is_Deleted != true);
                    viewModel.Locations = location?.Location_Code;
                    viewModel.Location_Name = asset.Location;
                    if (!string.IsNullOrEmpty(asset.City))
                    {
                        viewModel.LocationsList = db.Location
                            .Where(l => l.City_Name == asset.City && l.Is_Deleted != true)
                            .ToList();
                    }
                    viewModel.Last_Check_Date = asset.Last_Check_Date;
                    viewModel.Condition = asset.Condition;
                    viewModel.Status = asset.Status;
                    viewModel.PIC = asset.PIC;
                    viewModel.Transaction_Date = asset.Transaction_Date.HasValue ? (DateTime)asset.Transaction_Date : DateTime.Now;
                    viewModel.AssetHistory = db.Asset
                   .Where(a => a.No_asset == id && a.Is_Deleted != true)
                   .OrderByDescending(a => a.Transaction_Date)
                   .ToList();

                }
                else
                {
                    TempData["ErrorMessage"] = "Asset not found.";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                viewModel.AssetHistory = new List<Asset>();
            }

            ViewBag.Mode = mode;
            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editor(AssetManagementViewModel viewModel)
        {
            try
            {
                string mode = viewModel.mode ?? "Create";

                if (mode == "Delete")
                {
                    return DeleteAsset(viewModel.No_asset);
                }
                string locationName = null;
                if (!string.IsNullOrEmpty(viewModel.Locations))
                {
                    var location = db.Location.FirstOrDefault(l => l.Location_Code == viewModel.Locations && l.Is_Deleted != true);
                    locationName = location?.Location_Name;
                }



                string departmentName = null;
                if (!string.IsNullOrEmpty(viewModel.Departement_Code))
                {
                    var department = db.Departement.FirstOrDefault(d => d.Departement_Code == viewModel.Departement_Code && d.Is_Deleted != true);
                    departmentName = department?.Departement_Name;
                }

                if (string.IsNullOrEmpty(viewModel.No_asset))
                {
                    ModelState.AddModelError("No_asset", "No Asset is required");
                }
                if (string.IsNullOrEmpty(viewModel.Material_Code))
                {
                    ModelState.AddModelError("Material_Code", "Material Code is required");
                }
                if (string.IsNullOrEmpty(viewModel.Material_Group))
                {
                    ModelState.AddModelError("Material_Group", "Material Group is required");
                }
                if (string.IsNullOrEmpty(viewModel.Company_Code))
                {
                    ModelState.AddModelError("Company_Code", "Company Code is required");
                }
                if (string.IsNullOrEmpty(viewModel.Departement_Code))
                {
                    ModelState.AddModelError("Departement_Code", "Department is required");
                }
                if (string.IsNullOrEmpty(viewModel.City_Name))
                {
                    ModelState.AddModelError("City_Name", "City is required");
                }
                if (string.IsNullOrEmpty(viewModel.Locations))
                {
                    ModelState.AddModelError("Locations", "Location is required");
                }
                if (string.IsNullOrEmpty(viewModel.Material_Description))
                {
                    ModelState.AddModelError("Material_Description", "Asset Description is required");
                }
                if (string.IsNullOrEmpty(viewModel.Status))
                {
                    ModelState.AddModelError("Status", "Status is required");
                }
                if (string.IsNullOrEmpty(viewModel.PIC))
                {
                    ModelState.AddModelError("PIC", "PIC is required");
                }
                if (!viewModel.Transaction_Date.HasValue)
                {
                    ModelState.AddModelError("Transaction_Date", "Transaction Date is required");
                }
                if (ModelState.IsValid)
                {
                    var existAsset = db.Asset
                         .Where(a => a.No_asset == viewModel.No_asset && a.Is_Deleted != true)
                         .OrderByDescending(a => a.Transaction_Date)
                         .FirstOrDefault();


                    var assetHistory = new Asset
                    {
                        No_asset = viewModel.No_asset,
                        Company_Code = viewModel.Company_Code,
                        Company_Name = viewModel.Company_Name,
                        Material_Group = viewModel.Material_Group,
                        Material_Code = viewModel.Material_Code,
                        Material_Description = viewModel.Material_Description,
                        Quantity = viewModel.Quantity,
                        UoM = viewModel.UoM,
                        Serial_Number = viewModel.Serial_Number,
                        Device_Id = viewModel.Device_Id,
                        Acquisition_Date = viewModel.Acquisition_Date,
                        Acquisition_value = viewModel.Acquisition_value,
                        No_Asset_PGA = viewModel.No_Asset_PGA,
                        No_Asset_Accounting = viewModel.No_Asset_Accounting,
                        No_PO = viewModel.No_PO,
                        Latest_User = viewModel.Latest_User,
                        Departement = departmentName,
                        Location = locationName,
                        City = viewModel.City_Name,
                        Last_Check_Date = viewModel.Last_Check_Date,
                        Condition = viewModel.Condition,
                        Status = viewModel.Status,
                        PIC = viewModel.PIC,
                        Transaction_Date = viewModel.Transaction_Date,
                        Create_By = User.Identity.Name ?? "System",
                        Create_Date = DateTime.Now,
                        Is_Deleted = false

                    };

                    if (existAsset != null)
                    {
                        existAsset.Status = viewModel.Status;
                        existAsset.PIC = viewModel.PIC;
                        existAsset.Transaction_Date = viewModel.Transaction_Date;
                        existAsset.Edit_By = User.Identity.Name ?? "System";
                        existAsset.Edit_Date = DateTime.Now;
                        existAsset.Latest_User = viewModel.Latest_User;
                        existAsset.Location = locationName;
                        existAsset.Departement = departmentName;
                        existAsset.Condition = viewModel.Condition;
                    }
                    db.Asset.Add(assetHistory);
                    db.SaveChanges();

                    var dashboardCounts = GetDashboardCounts();

                    if (Request.IsAjaxRequest())
                    {
                        return Json(new
                        {
                            success = true,
                            message = mode == "Create" ? "Asset created successfully" : "Asset updated successfully",
                            assetData = new
                            {
                                assetHistory.No_asset,
                                assetHistory.PIC,
                                Transaction_Date = assetHistory.Transaction_Date.HasValue ? assetHistory.Transaction_Date.Value.ToString("yyyy-MM-dd") : "",
                                assetHistory.Status,
                                Submit_Date = assetHistory.Create_Date.HasValue ? assetHistory.Create_Date.Value.ToString("yyyy-MM-dd") : ""
                            },
                            dashboardCounts = dashboardCounts
                        });
                    }
                    else
                    {
                        TempData["SuccessMessage"] = mode == "Create" ? "Asset created successfully" : "Asset updated successfully";
                        TempData["DashboardCounts"] = dashboardCounts;
                        return RedirectToAction("Index");
                    }
                }

                LoadDropdownData();

                viewModel.Companies = db.Company.Where(c => c.Is_Deleted != true).ToList();
                viewModel.Dept = db.Departement.Where(c => c.Is_Deleted != true).ToList();
                viewModel.LocationsList = db.Location.Where(c => c.Is_Deleted != true).ToList();
                viewModel.Cities = db.City.Where(c => c.Is_Deleted != true).ToList();
                viewModel.MaterialGroup = db.Material_Group.Where(c => c.Is_Deleted != true).ToList();
                viewModel.Material_Code1 = db.Material_Code.Where(c => c.Is_Deleted != true).ToList();
                viewModel.UoMList = db.UoM.Where(c => c.Is_Deleted != true).ToList();

                if (mode != "Create" && !string.IsNullOrEmpty(viewModel.No_asset))
                {
                    viewModel.AssetHistory = db.Asset
               .Where(a => a.No_asset == viewModel.No_asset && a.Is_Deleted != true)
               .OrderByDescending(a => a.Transaction_Date)
               .ToList();
                }
                else
                {
                    viewModel.AssetHistory = new List<Asset>();
                }

                viewModel.DashboardCounts = GetDashboardCounts();
                ViewBag.Mode = mode;
                if (Request.IsAjaxRequest())
                {
                    var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );

                    return Json(new { success = false, message = "Validation failed", errors = errors });
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                if (Request.IsAjaxRequest())
                {

                    return Json(new
                    {
                        success = false,
                        message = "An error occurred while processing your request: " + ex.Message,
                        details = ex.ToString()
                    });
                }

                string mode = viewModel.mode ?? "Create";

                if (mode != "Create" && !string.IsNullOrEmpty(viewModel.No_asset))
                {
                    viewModel.AssetHistory = db.Asset
                        .Where(a => a.No_asset == viewModel.No_asset && a.Is_Deleted != true)
                        .OrderByDescending(a => a.Transaction_Date)
                        .ToList();
                }
                else
                {
                    viewModel.AssetHistory = new List<Asset>();
                }
                ModelState.AddModelError("", "An error occurred while processing your request: " + ex.Message);
                viewModel.Companies = db.Company.Where(c => c.Is_Deleted != true).ToList();
                viewModel.Dept = db.Departement.Where(c => c.Is_Deleted != true).ToList();
                viewModel.LocationsList = db.Location.Where(c => c.Is_Deleted != true).ToList();
                viewModel.Cities = db.City.Where(c => c.Is_Deleted != true).ToList();
                viewModel.MaterialGroup = db.Material_Group.Where(c => c.Is_Deleted != true).ToList();
                viewModel.Material_Code1 = db.Material_Code.Where(c => c.Is_Deleted != true).ToList();
                viewModel.UoMList = db.UoM.Where(c => c.Is_Deleted != true).ToList();
                viewModel.DashboardCounts = GetDashboardCounts();

                ViewBag.Mode = mode;
                LoadDropdownData();
                return View(viewModel);
            }
        }



        public string GetCompanyName(string companyCode)
        {
            var company = db.Company.FirstOrDefault(c => c.Company_Code == companyCode);
            return company?.Company_Name ?? string.Empty;
        }

        public JsonResult GetCompanyNameByCode(string companyCode)
        {
            try
            {
                if (string.IsNullOrEmpty(companyCode))
                {
                    return Json(string.Empty, JsonRequestBehavior.AllowGet);
                }

                var company = db.Company.FirstOrDefault(c => c.Company_Code == companyCode && c.Is_Deleted != true);
                string companyName = company?.Company_Name ?? string.Empty;

                return Json(companyName, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return Json(string.Empty, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetAssetDetails(string assetId)
        {
            try
            {
                if (string.IsNullOrEmpty(assetId))
                {
                    return Json(new { success = false, message = "Asset ID is required." }, JsonRequestBehavior.AllowGet);
                }
                var asset = db.Asset
                    .Where(a => a.No_asset == assetId && a.Is_Deleted != true)
                    .OrderByDescending(a => a.Transaction_Date)
                    .FirstOrDefault();
                if (asset == null)
                {
                    return Json(new { success = false, message = "Asset not found." }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = true,
                    No_asset = asset.No_asset,
                    Company_Code = asset.Company_Code,
                    Company_Name = asset.Company_Name,
                    Material_Group = asset.Material_Group,
                    Material_Code = asset.Material_Code,
                    Material_Description = asset.Material_Description,
                    Quantity = asset.Quantity,
                    UoM = asset.UoM,
                    Serial_Number = asset.Serial_Number,
                    Device_Id = asset.Device_Id,
                    Acquisition_Date = asset.Acquisition_Date,
                    Acquisition_value = asset.Acquisition_value,
                    PIC = asset.PIC,
                    Status = asset.Status,
                    Transaction_Date = asset.Transaction_Date,
                    Location = asset.Location,
                    Departement = asset.Departement,
                    Condition = asset.Condition
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while retrieving asset details: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateAsset(string No_asset, string Status, string PIC, DateTime? Transaction_Date)
        {
            try
            {

                if (string.IsNullOrEmpty(No_asset))
                {
                    return Json(new { success = false, message = "Asset number is required" });
                }

                var asset = db.Asset
                    .Where(a => a.No_asset == No_asset && a.Is_Deleted != true)
                    .OrderByDescending(a => a.Transaction_Date)
                    .FirstOrDefault();
                if (asset == null)
                {
                    return Json(new { success = false, message = "Asset not found" });
                }
                if (string.IsNullOrEmpty(Status))
                {
                    return Json(new { success = false, message = "Status is required" });
                }

                var validStatus = new[] { "Return", "Borrowing", "Service", "Ready", "Assign", "Write Off" };
                if (!validStatus.Contains(Status))
                {
                    return Json(new { success = false, message = "Invalid status selected" });
                }

                if (string.IsNullOrEmpty(PIC))
                {
                    return Json(new { success = false, message = $"PIC for {Status} status is required" });
                }

                if (!Transaction_Date.HasValue)
                {
                    return Json(new { success = false, message = $"Transaction Date for {Status} status is required" });
                }

                var newTransaction = new Asset
                {
                    No_asset = asset.No_asset,
                    Company_Code = asset.Company_Code,
                    Company_Name = asset.Company_Name,
                    Material_Group = asset.Material_Group,
                    Material_Code = asset.Material_Code,
                    Material_Description = asset.Material_Description,
                    Quantity = asset.Quantity,
                    UoM = asset.UoM,
                    Serial_Number = asset.Serial_Number,
                    Device_Id = asset.Device_Id,
                    Acquisition_Date = asset.Acquisition_Date,
                    Acquisition_value = asset.Acquisition_value,
                    No_Asset_PGA = asset.No_Asset_PGA,
                    No_Asset_Accounting = asset.No_Asset_Accounting,
                    No_PO = asset.No_PO,
                    Latest_User = asset.Latest_User,
                    Departement = asset.Departement,
                    Location = asset.Location,
                    City = asset.City,
                    Last_Check_Date = asset.Last_Check_Date,
                    Condition = asset.Condition,

                    Status = Status,
                    PIC = PIC,
                    Transaction_Date = Transaction_Date,
                    Create_By = User.Identity.Name ?? "System",
                    Create_Date = DateTime.Now,
                    Is_Deleted = false
                };
                db.Asset.Add(newTransaction);
                asset.Status = Status;
                asset.PIC = PIC;
                asset.Transaction_Date = Transaction_Date;
                asset.Edit_By = User.Identity.Name ?? "System";
                asset.Edit_Date = DateTime.Now;
                db.SaveChanges();

                var dashboardCounts = GetDashboardCounts();

                return Json(new
                {
                    success = true,
                    message = "Asset updated successfully",
                    assetData = new
                    {
                        No_asset = newTransaction.No_asset,
                        PIC = newTransaction.PIC,
                        Transaction_Date = newTransaction.Transaction_Date.HasValue ? newTransaction.Transaction_Date.Value.ToString("yyyy-MM-dd") : "",
                        Status = newTransaction.Status,
                        Submit_Date = newTransaction.Create_Date.HasValue ? newTransaction.Create_Date.Value.ToString("yyyy-MM-dd") : ""
                    },
                    dashboardCounts = dashboardCounts
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating asset: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Json(new
                {
                    success = false,
                    message = "An error occurred while updating the asset",
                    errorDetails = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        public ActionResult DeleteAsset(string No_asset)
        {
            try
            {
                if (string.IsNullOrEmpty(No_asset))
                {
                    if (Request.IsAjaxRequest())

                        return Json(new { success = false, message = "Asset number is required" });

                    TempData["ErrorMessage"] = "Asset number is required";
                    return RedirectToAction("Index");

                }
                var assetExists = db.Asset.Any(a => a.No_asset == No_asset && a.Is_Deleted != true);
                if (!assetExists)
                {
                    if (Request.IsAjaxRequest())
                        return Json(new { success = false, message = "Asset not found" });

                    TempData["ErrorMessage"] = "Asset not found";
                    return RedirectToAction("Index");
                }

                var assetRecords = db.Asset.Where(a => a.No_asset == No_asset && a.Is_Deleted != true).ToList();

                foreach (var asset in assetRecords)
                {
                    asset.Is_Deleted = true;
                    asset.Delete_By = User.Identity.Name ?? "System";
                    asset.Delete_Date = DateTime.Now;
                }

                db.SaveChanges();
                var dashboardCounts = GetDashboardCounts();

                if (Request.IsAjaxRequest())
                {
                    return Json(new
                    {
                        success = true,
                        message = "Asset deleted successfully",
                        dashboardCounts = dashboardCounts
                    });
                }

                TempData["SuccessMassage"] = "Asset deleted successfully";
                TempData["DashboardCounts"] = dashboardCounts;
                return RedirectToAction("Index");
            }

            catch (Exception ex)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "An error occurred while deleting the asset: " + ex.Message });
                }

                TempData["ErrorMassage"] = "An error occurred while deleting the asset: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        public ActionResult GetAssetData(string search)
        {
            var assetData = db.Asset.Where(a => a.Is_Deleted != true);
            if (!string.IsNullOrEmpty(search))
            {
                assetData = assetData.Where(a => a.No_asset.Contains(search) || a.PIC.Contains(search) || a.Status.Contains(search));
            }

            var latestAssets = assetData
                .GroupBy(a => a.No_asset)
                .Select(g => g.OrderByDescending(a => a.Transaction_Date).FirstOrDefault());
            var assets = latestAssets.OrderByDescending(a => a.Transaction_Date).Select(a => new
            {
                a.No_asset,
                a.Company_Name,
                a.Material_Group,
                a.Material_Code,
                a.Material_Description,
                a.Departement,
                a.Location,
                a.PIC,
                a.Transaction_Date,
                a.Status,
                Submit_Date = a.Create_Date
            }).Take(100)
              .ToList();
            return Json(assets, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SearchAssetHistory(string search)
        {
            var assetData = db.Asset.Where(a => a.Is_Deleted != true);
            if (!string.IsNullOrEmpty(search))
            {
                assetData = assetData.Where(a => a.No_asset.Contains(search) || a.PIC.Contains(search) || a.Status.Contains(search) || (a.Transaction_Date.HasValue && a.Transaction_Date.Value.ToString().Contains(search)));
            }
            var assets = assetData.OrderByDescending(a => a.Transaction_Date).Select(a => new
            {
                a.No_asset,
                a.PIC,
                a.Transaction_Date,
                a.Status,
                Submit_Date = a.Create_Date
            }).Take(100)
              .ToList();
            return Json(assets, JsonRequestBehavior.AllowGet);
        }

        private DashboardCountsModel GetDashboardCounts()
        {

            var latestAssets = db.Asset
                .Where(a => a.Is_Deleted != true)
                .GroupBy(a => a.No_asset)
                .Select(g => g.OrderByDescending(a => a.Transaction_Date).FirstOrDefault())
                .ToList();

            return new DashboardCountsModel
            {
                TotalAssets = latestAssets
                    .Where(a => a.Status != "Write Off")
                    .Count(),

                AvailableAssets = latestAssets
                    .Where(a => a.Status == "Ready" || a.Status == "Return")
                    .Count(),

                AssetsInUse = latestAssets
                    .Where(a => a.Status == "Borrowing" || a.Status == "Assign")
                    .Count(),

                AssetsInMaintenance = latestAssets
                    .Where(a => a.Status == "Service")
                    .Count()
            };
        }

        [HttpGet]
        public JsonResult GetLocationByCity(string cityId)
        {
            var locations = db.Location
                .Where(l => l.City_Name == cityId && l.Is_Deleted != true)
                .Select(l => new
                {
                    locationCode = l.Location_Code,
                    locationName = l.Location_Name
                })
                .ToList();
            return Json(locations, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        private void LoadDropdownData()
        {
            ViewBag.Company = db.Company.Where(c => c.Is_Deleted != true).ToList();
            ViewBag.Departement = db.Departement.Where(c => c.Is_Deleted != true).ToList();
            ViewBag.Location = db.Location.Where(c => c.Is_Deleted != true).ToList();
            ViewBag.City = db.City.Where(c => c.Is_Deleted != true).ToList();
            ViewBag.MaterialGroup = db.Material_Group.Where(c => c.Is_Deleted != true).ToList();
            ViewBag.Material_Code1 = db.Material_Code.Where(c => c.Is_Deleted != true).ToList();
            ViewBag.UoMList = db.UoM.Where(c => c.Is_Deleted != true).ToList();

            ViewBag.Status = new List<SelectListItem>
            {
                new SelectListItem { Text = "Select Status", Value = "" },
                new SelectListItem { Text = "Return", Value = "Return" },
                new SelectListItem { Text = "Borrowing", Value = "Borrowing" },
                new SelectListItem { Text = "Service", Value = "Service" },
                new SelectListItem { Text = "Ready", Value = "Ready" },
                new SelectListItem { Text = "Assign", Value = "Assign" },
                new SelectListItem { Text = "Write Off", Value = "Write Off" }
            };
        }
    }
}