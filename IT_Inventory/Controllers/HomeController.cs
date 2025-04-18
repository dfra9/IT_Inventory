using System;
using System.Linq;
using System.Linq.Expressions;
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


        [HttpPost]
        public ActionResult GetDashboardData(DataTablesParameters parameters)
        {
            try
            {

                var query = db.Asset.AsQueryable();

                query = query.Where(a => a.Is_Deleted != true);

                if (!string.IsNullOrEmpty(parameters.Search?.Value))
                {
                    var search = parameters.Search.Value.ToLower();
                    query = query.Where(a =>
                        (a.No_asset != null && a.No_asset.ToLower().Contains(search)) ||
                        (a.Material_Group != null && a.Material_Group.ToLower().Contains(search)) ||
                        (a.Material_Description != null && a.Material_Description.ToLower().Contains(search)) ||
                        (a.Location != null && a.Location.ToLower().Contains(search)) ||
                        (a.Departement != null && a.Departement.ToLower().Contains(search)) ||
                        (a.Status != null && a.Status.ToLower().Contains(search))
                    );
                }

                if (parameters.Columns != null)
                {
                    for (int i = 0; i < parameters.Columns.Length; i++)
                    {
                        var column = parameters.Columns[i];
                        if (column.Searchable && !string.IsNullOrEmpty(column.Search?.Value))
                        {
                            var searchValue = column.Search.Value.ToLower();
                            query = ApplyColumnSearch(query, column.Data, searchValue);
                        }
                    }
                }

                int totalRecords = db.Asset.Count(a => a.Is_Deleted != true);

                if (parameters.Order != null && parameters.Order.Length > 0)
                {
                    var sortColumn = parameters.Columns[parameters.Order[0].Column].Data;
                    var sortDirection = parameters.Order[0].Dir;

                    try
                    {
                        query = ApplySorting(query, sortColumn, sortDirection);
                    }
                    catch (Exception ex)
                    {

                        System.Diagnostics.Debug.WriteLine($"Sorting error: {ex.Message}");
                        query = query.OrderByDescending(a => a.Transaction_Date);
                    }
                }
                else
                {

                    query = query.OrderByDescending(a => a.Transaction_Date);
                }

                int filteredRecords = query.Count();

                var pagedQuery = query
                    .Skip(parameters.Start)
                    .Take(parameters.Length);

                var data = pagedQuery
                    .Select(a => new
                    {
                        a.ID,
                        a.No_asset,
                        a.Company_Code,
                        a.Material_Group,
                        a.Material_Description,
                        a.Acquisition_Date,
                        a.Status,
                        a.Departement,
                        a.Location,
                        a.Is_Deleted
                    })
                    .ToList();

                return Json(new
                {
                    draw = parameters.Draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = filteredRecords,
                    data = data
                });
            }
            catch (Exception ex)
            {

                System.Diagnostics.Debug.WriteLine($"Error in GetDashboardData: {ex.Message}");

                return Json(new
                {
                    draw = parameters.Draw,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new object[0],
                    error = ex.Message
                });
            }
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

                // Untuk kolom lainnya
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