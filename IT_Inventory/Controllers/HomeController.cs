using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using IT_Inventory.Models;
using IT_Inventory.ViewModel;
using static IT_Inventory.ViewModel.AssetManagementViewModel;


namespace IT_Inventory.Controllers
{
    public class HomeController : Controller
    {
        private readonly DBIT_Inventory db = new DBIT_Inventory();

        [Authorize]
        public ActionResult Index()
        {
            ViewBag.City = db.City.Where(c => c.Is_Deleted != true).ToList();

            var latestAssets = db.Asset
       .Where(a => a.Is_Deleted != true)
       .GroupBy(a => a.No_asset)
       .Select(g => g.OrderByDescending(a => a.Transaction_Date).FirstOrDefault())
       .ToList();
            var dashboardCounts = new DashboardCountsModel
            {
                TotalAssets = latestAssets.Count,
                AvailableAssets = latestAssets.Count(a => a.Status == "Ready" || a.Status == "Return"),
                AssetsInUse = latestAssets.Count(a => a.Status == "Borrowing" || a.Status == "Assign"),
                AssetsInMaintenance = latestAssets.Count(a => a.Status == "Service")
            };


            var viewModel = new AssetManagementViewModel
            {
                DashboardCounts = dashboardCounts,
                DashHistory = GetDashboardHistory()
            };

            return View(viewModel);

        }


        [HttpGet]
        public ActionResult GetDashboardCountsJson(string cityName = "")
        {
            var assetQuery = db.Asset.Where(a => a.Is_Deleted != true);
            if (!string.IsNullOrEmpty(cityName))
            {
                assetQuery = assetQuery.Where(a => a.City == cityName);
            }

            var latestAsset = assetQuery
                .GroupBy(a => a.No_asset)
                .Select(g => g.OrderByDescending(a => a.Transaction_Date).FirstOrDefault())
                .ToList();

            var dashboardCounts = new DashboardCountsModel
            {
                TotalAssets = latestAsset.Count(a => a.Status != "Write Off"),
                AvailableAssets = latestAsset.Count(a => a.Status == "Ready" || a.Status == "Return"),
                AssetsInUse = latestAsset.Count(a => a.Status == "Borrowing" || a.Status == "Assign"),
                AssetsInMaintenance = latestAsset.Count(a => a.Status == "Service")
            };
            return Json(dashboardCounts, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetDashboardData(DataTablesParameters param)
        {
            try
            {
                int totalCount;
                int filteredCount;
                var assets = GetFilteredAssets(param, out totalCount, out filteredCount);
                return Json(new
                {
                    draw = param.Draw,
                    recordsTotal = totalCount,
                    recordsFiltered = filteredCount,
                    data = assets.Select(a => new
                    {
                        a.No_asset,
                        a.Material_Group,
                        a.Material_Description,
                        Transaction_Date = a.Transaction_Date,
                        a.Status,
                        a.Location,
                        a.Departement,
                        a.Is_Deleted
                    })
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = "Error getting dashboard data: " + ex.Message });
            }
        }

        private List<Asset> GetDashboardHistory()
        {
            return db.Asset
                .Where(a => a.Is_Deleted != true)
                .GroupBy(a => a.No_asset)
                .Select(g => g.OrderByDescending(a => a.Transaction_Date).FirstOrDefault())
                .OrderByDescending(a => a.Transaction_Date)
                .Take(10)
                .ToList();
        }

        private List<Asset> GetFilteredAssets(DataTablesParameters param, out int totalCount, out int filteredCount)
        {

            var assetQuery = db.Asset
                .Where(a => a.Is_Deleted != true)
                .GroupBy(a => a.No_asset)
                .Select(g => g.OrderByDescending(a => a.Transaction_Date).FirstOrDefault())
                .AsQueryable();

            if (!string.IsNullOrEmpty(param.cityName))
            {
                assetQuery = assetQuery.Where(a => a.City == param.cityName);
            }
            totalCount = assetQuery.Count();

            if (!string.IsNullOrEmpty(param.globalSearch))
            {
                string search = param.globalSearch.ToLower();
                assetQuery = assetQuery.Where(a =>
                    a.No_asset.ToLower().Contains(search) ||
                    (a.Material_Group != null && a.Material_Group.ToLower().Contains(search)) ||
                    (a.Material_Description != null && a.Material_Description.ToLower().Contains(search)) ||
                    (a.Status != null && a.Status.ToLower().Contains(search)) ||
                    (a.Location != null && a.Location.ToLower().Contains(search)) ||
                    (a.Departement != null && a.Departement.ToLower().Contains(search))
                );
            }

            if (param.Columns != null)
            {
                for (int i = 0; i < param.Columns.Length; i++)
                {
                    var column = param.Columns[i];
                    if (!string.IsNullOrEmpty(column.Search?.Value))
                    {
                        assetQuery = ApplyColumnSearch(assetQuery, column.Data, column.Search.Value);
                    }
                }
            }

            filteredCount = assetQuery.Count();

            if (param.Order != null && param.Order.Length > 0)
            {
                var orderParam = param.Order[0];
                var sortColumn = param.Columns[orderParam.Column].Data;
                assetQuery = ApplySorting(assetQuery, sortColumn, orderParam.Dir);
            }
            else
            {
                assetQuery = assetQuery.OrderByDescending(a => a.Transaction_Date);
            }

            return assetQuery
                .Skip(param.Start)
                .Take(param.Length)
                .ToList();
        }

        private IQueryable<Asset> ApplyColumnSearch(IQueryable<Asset> query, string columnName, string searchValue)
        {
            searchValue = searchValue.ToLower();
            switch (columnName)
            {
                case "No_asset":
                    return query.Where(a => (a.No_asset ?? "").ToLower().Contains(searchValue));
                case "Material_Group":
                    return query.Where(a => (a.Material_Group ?? "").ToLower().Contains(searchValue));
                case "Material_Description":
                    return query.Where(a => (a.Material_Description ?? "").ToLower().Contains(searchValue));
                case "Location":
                    return query.Where(a => (a.Location ?? "").ToLower().Contains(searchValue));
                case "Departement":
                    return query.Where(a => (a.Departement ?? "").ToLower().Contains(searchValue));
                case "Status":
                    return query.Where(a => (a.Status ?? "").ToLower().Contains(searchValue));
                case "Transaction_Date":
                    if (DateTime.TryParse(searchValue, out DateTime date))
                    {
                        return query.Where(a => a.Transaction_Date.HasValue &&
                                           a.Transaction_Date.Value.Year == date.Year &&
                                           a.Transaction_Date.Value.Month == date.Month &&
                                           a.Transaction_Date.Value.Day == date.Day);
                    }
                    return query;
                default:
                    return query;
            }
        }

        private IQueryable<Asset> ApplySorting(IQueryable<Asset> query, string sortColumn, string sortDirection)
        {
            try
            {

                switch (sortColumn)
                {
                    case "No_asset":
                        return sortDirection.ToLower() == "asc"
                            ? query.OrderBy(a => a.No_asset)
                            : query.OrderByDescending(a => a.No_asset);
                    case "Material_Group":
                        return sortDirection.ToLower() == "asc"
                            ? query.OrderBy(a => a.Material_Group ?? string.Empty)
                            : query.OrderByDescending(a => a.Material_Group ?? string.Empty);
                    case "Material_Description":
                        return sortDirection.ToLower() == "asc"
                            ? query.OrderBy(a => a.Material_Description ?? string.Empty)
                            : query.OrderByDescending(a => a.Material_Description ?? string.Empty);
                    case "Transaction_Date":
                        return sortDirection.ToLower() == "asc"
                            ? query.OrderBy(a => a.Transaction_Date)
                            : query.OrderByDescending(a => a.Transaction_Date);
                    case "Status":
                        return sortDirection.ToLower() == "asc"
                            ? query.OrderBy(a => a.Status ?? string.Empty)
                            : query.OrderByDescending(a => a.Status ?? string.Empty);
                    case "Location":
                        return sortDirection.ToLower() == "asc"
                            ? query.OrderBy(a => a.Location ?? string.Empty)
                            : query.OrderByDescending(a => a.Location ?? string.Empty);
                    case "Departement":
                        return sortDirection.ToLower() == "asc"
                            ? query.OrderBy(a => a.Departement ?? string.Empty)
                            : query.OrderByDescending(a => a.Departement ?? string.Empty);
                    default:
                        return query.OrderByDescending(a => a.Transaction_Date);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Sorting error: {ex.Message}");
                return query.OrderByDescending(a => a.Transaction_Date);
            }
        }


        public class DataTablesParameters
        {
            public int Draw { get; set; }
            public int Start { get; set; }
            public int Length { get; set; }
            public SearchParameters Search { get; set; }
            public OrderParameters[] Order { get; set; }
            public ColumnParameters[] Columns { get; set; }

            public string globalSearch { get; set; }
            public string cityName { get; set; }
        }

        public class SearchParameters
        {
            public string Value { get; set; }
            public bool Regex { get; set; }
        }

        public class OrderParameters
        {
            public int Column { get; set; }
            public string Dir { get; set; }
        }

        public class ColumnParameters
        {
            public string Data { get; set; }
            public string Name { get; set; }
            public bool Searchable { get; set; }
            public bool Orderable { get; set; }
            public SearchParameters Search { get; set; }
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