using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using ClosedXML.Excel;
using DBIT_Inventory.Services;
using DBIT_Inventory.Utilities;
using DBIT_Inventory.ViewModel;


namespace DBIT_Inventory.Controllers
{
    public class ReportAssetController : Controller
    {
        private readonly DBIT_Inventory _db;
        private readonly AssetService _assetService;
        public ReportAssetController(DBIT_Inventory db, IAssetService assetService)
        {
            _db = new DBIT_Inventory();
            _assetService = new AssetService(_db);
        }

        public void LoadDropdownData()
        {
            DropdownLoaderUtility.LoadAllDropdownData(_db, ViewBag);
        }

        public ActionResult Index()
        {
            var viewModel = new ReportAssetViewModel();

            LoadDropdownData(viewModel);

            return View(viewModel);
        }

        public ActionResult GetReport(ReportAssetViewModel model)
        {
            if (string.IsNullOrEmpty(model.No_asset) &&
                string.IsNullOrEmpty(model.Company_Code) &&
                string.IsNullOrEmpty(model.Material_Group) &&
                string.IsNullOrEmpty(model.Material_Code) &&
                string.IsNullOrEmpty(model.Acquisition_Value))
            {

                ModelState.AddModelError("", "Please select at least one filter option before generating the report.");
            }
            else
            {

                model.ReportItems = _assetService.GetAssetReports(
                    model.No_asset,
                    model.Company_Code,
                    model.Material_Group,
                    model.Material_Code,
                    model.Acquisition_Value
                );
            }

            LoadDropdownData(model);

            return View("Index", model);
        }


        public void LoadDropdownData(ReportAssetViewModel model)
        {
            var assetNumbers = _assetService.GetDistinctAssetNumbers();
            model.AssetNoList = new SelectList(assetNumbers, model.No_asset);

            var companies = _assetService.GetActiveCompanies();
            model.CompanyList = new SelectList(companies, "Company_Code", "Company_Name", model.Company_Code);

            var materialGroups = _assetService.GetDistinctMaterialGroups();
            model.MaterialGroupList = new SelectList(materialGroups, model.Material_Group);

            var materialCodes = _assetService.GetDistinctMaterialCodes();
            model.MaterialCodeList = new SelectList(materialCodes, model.Material_Code);

            var acquisitionValues = _assetService.GetDistinctAcquisitionValues();
            model.AcquisitionValueList = new SelectList(acquisitionValues, model.Acquisition_Value);

        }

        public ActionResult ExportToExcel(ReportAssetViewModel model)
        {
            var reportItems = _assetService.GetAssetReports(
                model.No_asset,
                model.Company_Code,
                model.Material_Group,
                model.Material_Code,
                model.Acquisition_Value
                );

            if (reportItems == null || !reportItems.Any())
            {
                TempData["ErrorMessage"] = "No data to export.";
                return RedirectToAction("Index");
            }

            DataTable dt = new DataTable("Asset Report");

            dt.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Asset No"),
                new DataColumn("Company Code"),
                new DataColumn("Company Name"),
                new DataColumn("Material Group"),
                new DataColumn("Material Code"),
                new DataColumn("Asset Description"),
                new DataColumn("Quantity"),
                new DataColumn("UoM"),
                new DataColumn("Acquisition Date"),
                new DataColumn("Acquisition Value"),
                new DataColumn("No Asset PGA"),
                new DataColumn("No Asset Accounting"),
                new DataColumn("No PO"),
                new DataColumn("Serial Number"),
                new DataColumn("Device Id"),
                new DataColumn("Latest User"),
                new DataColumn("Department"),
                new DataColumn("City"),
                new DataColumn("Location"),
                new DataColumn("Condition"),
                new DataColumn("Last Check Date"),
                new DataColumn("Status"),
                new DataColumn("PIC"),
                new DataColumn("Role"),
                new DataColumn("Company User"),
                new DataColumn("Submit Date")

            });


            foreach (var item in reportItems)
            {
                dt.Rows.Add(
                    item.No_asset,
                    item.Company_Code,
                    item.Company_Name,
                    item.Material_Group,
                    item.Material_Code,
                    item.Material_Description,
                    item.Quantity,
                    item.UoM,
                    item.Acquisition_Date?.ToString("yyyy-MM-dd"),
                    item.Acquisition_Value,
                    item.No_Asset_PGA,
                    item.No_Asset_Accounting,
                    item.No_PO,
                    item.Serial_Number,
                    item.Device_Id,
                    item.Latest_User,
                    item.Dept,
                    item.Cities,
                    item.Locations,
                    item.Condition,
                    item.Last_Check_Date?.ToString("yyyy-MM-dd"),
                    item.Status,
                    item.PIC,
                    item.Role,
                    item.Company_User,
                    item.Create_Date?.ToString("yyyy-MM-dd")
                    );
            }
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add(dt);

                var headerRow = ws.Row(1);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#4F81BD");

                ws.Columns().AdjustToContents();

                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    ms.Position = 0;
                    string fileName = "Asset_Report_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
                    return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }

            }

        }
    }
}