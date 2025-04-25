using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using IT_Inventory.Models;
using IT_Inventory.ViewModel;
using static IT_Inventory.ViewModel.AssetManagementViewModel;


namespace IT_Inventory.Controllers
{
    public class HomeController : Controller
    {
        private readonly IT_Inventory db = new IT_Inventory();

        [Authorize]
        public ActionResult Index()
        {
            var dashboardCounts = new DashboardCountsModel
            {
                TotalAssets = db.Asset
                    .Where(a => a.Is_Deleted != true && a.Status != "Write Off")
                    .GroupBy(a => a.No_asset)
                    .Select(g => g.OrderByDescending(a => a.Transaction_Date).FirstOrDefault())
                    .Count(),

                AvailableAssets = db.Asset
                    .Where(a => a.Is_Deleted != true && (a.Status == "Ready" || a.Status == "Return"))
                    .GroupBy(a => a.No_asset)
                    .Select(g => g.OrderByDescending(a => a.Transaction_Date).FirstOrDefault())
                    .Count(),

                AssetsInUse = db.Asset
                    .Where(a => a.Is_Deleted != true && (a.Status == "Borrowing" || a.Status == "Assign"))
                    .GroupBy(a => a.No_asset)
                    .Select(g => g.OrderByDescending(a => a.Transaction_Date).FirstOrDefault())
                    .Count(),

                AssetsInMaintenance = db.Asset
                    .Where(a => a.Is_Deleted != true && a.Status == "Service")
                    .GroupBy(a => a.No_asset)
                    .Select(g => g.OrderByDescending(a => a.Transaction_Date).FirstOrDefault())
                    .Count()
            };

            var viewModel = new AssetManagementViewModel
            {
                DashboardCounts = dashboardCounts,
                DashHistory = GetDashboardHistory()
            };

            return View(viewModel);
            return View();
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
                    data = assets
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = "Error getting dashboard data: " + ex.Message });
            }
        }

        private List<Asset> GetFilteredAssets(DataTablesParameters param, out int totalCount, out int filteredCount)
        {
            var assetQuery = db.Asset
                .Where(a => a.Is_Deleted != true)
                .GroupBy(a => a.No_asset)
                .Select(g => g.OrderByDescending(a => a.Transaction_Date).FirstOrDefault())
                .AsQueryable();

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

            filteredCount = assetQuery.Count();

            return assetQuery
                .OrderByDescending(a => a.Transaction_Date)
                .Skip(param.Start)
                .Take(param.Length)
                .ToList();
        }

        private IQueryable<Asset> ApplyColumnSearch(IQueryable<Asset> query, string columnName, string searchValue)
        {
            switch (columnName)
            {
                case "No_asset":
                    return query.Where(a => a.No_asset != null && a.No_asset.ToLower().Contains(searchValue));
                case "Material_Group":
                    return query.Where(a => a.Material_Group != null && a.Material_Group.ToLower().Contains(searchValue));
                case "Material_Description":
                    return query.Where(a => a.Material_Description != null && a.Material_Description.ToLower().Contains(searchValue));
                case "Location":
                    return query.Where(a => a.Location != null && a.Location.ToLower().Contains(searchValue));
                case "Departement":
                    return query.Where(a => a.Departement != null && a.Departement.ToLower().Contains(searchValue));
                case "Status":
                    return query.Where(a => a.Status != null && a.Status.ToLower().Contains(searchValue));
                case "Acquisition_Date":

                    if (DateTime.TryParse(searchValue, out DateTime date))
                    {
                        return query.Where(a => a.Acquisition_Date.HasValue &&
                                           a.Acquisition_Date.Value.Year == date.Year &&
                                           a.Acquisition_Date.Value.Month == date.Month &&
                                           a.Acquisition_Date.Value.Day == date.Day);
                    }
                    return query.Where(a => a.Acquisition_Date.HasValue &&
                                       a.Acquisition_Date.Value.ToString().Contains(searchValue));
                default:
                    return query;
            }
        }

        private IQueryable<Asset> ApplySorting(IQueryable<Asset> query, string sortColumn, string sortDirection)
        {
            try
            {
                ParameterExpression parameter = Expression.Parameter(typeof(Asset), "a");


                if (sortColumn == "Acquisition_Date")
                {
                    if (sortDirection.ToLower() == "asc")
                        return query.OrderBy(a => a.Acquisition_Date);
                    else
                        return query.OrderByDescending(a => a.Acquisition_Date);
                }

                MemberExpression property = Expression.Property(parameter, sortColumn);
                var lambda = Expression.Lambda(property, parameter);
                string methodName = sortDirection.ToLower() == "asc" ? "OrderBy" : "OrderByDescending";

                var result = query.Provider.CreateQuery(
                    Expression.Call(
                        typeof(Queryable),
                        methodName,
                        new Type[] { query.ElementType, property.Type },
                        query.Expression,
                        Expression.Quote(lambda)
                    )
                );

                return (IQueryable<Asset>)result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Sorting error: {ex.Message}");
                return sortDirection.ToLower() == "asc"
                    ? query.OrderBy(a => a.No_asset)
                    : query.OrderByDescending(a => a.No_asset);
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