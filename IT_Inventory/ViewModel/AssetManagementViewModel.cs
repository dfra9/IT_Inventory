using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using IT_Inventory.Models;

namespace IT_Inventory.ViewModel
{
    public class AssetManagementViewModel
    {
        [Required(ErrorMessage = "No Asset is required")]
        public string No_asset { get; set; }
        [Required(ErrorMessage = "Company Code is required")]
        public string Company_Code { get; set; }
        public string Company_Name { get; set; }
        public string Material_Group_Code { get; set; }
        public string Material_Group { get; set; }
        public string Material_Description { get; set; }
        public int? Quantity { get; set; }
        public string Unit { get; set; }
        public DateTime? Acquisition_Date { get; set; }
        public string Acquisition_value { get; set; }
        public string No_Asset_PGA { get; set; }
        public string No_Asset_Accounting { get; set; }
        public string No_PO { get; set; }
        public string Latest_User { get; set; }


        public string Departement_Code { get; set; }

        public string Departement_Name { get; set; }
        public string Location_Code { get; set; }
        public string Location_Name { get; set; }
        public string City_Id { get; set; }

        public DateTime? Last_Check_Date { get; set; }
        public string Condition { get; set; }
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }
        [Required(ErrorMessage = "PIC is required")]
        public string PIC { get; set; }
        public string Vendor { get; set; }
        [Required(ErrorMessage = "Transaction Date is required")]
        public DateTime? Transaction_Date { get; set; }

        public string Create_By { get; set; }
        public DateTime? Create_Date { get; set; }
        public string Edit_By { get; set; }
        public DateTime? Edit_Date { get; set; }
        public string Delete_By { get; set; }
        public DateTime? Delete_Date { get; set; }
        public Nullable<bool> Is_Deleted { get; set; }

        public List<Company> Companies { get; set; }
        public List<Departement> Dept { get; set; }
        public List<Location> Locations { get; set; }
        public List<City> Cities { get; set; }
        public List<SelectListItem> StatusList { get; set; }

        public List<Asset> AssetHistory { get; set; }

        public List<Asset> DashHistory { get; set; }

        public int TotalAssets { get; set; }
        public int AvailableAssets { get; set; }
        public int AssetInUse { get; set; }
        public int AssetInService { get; set; }
        public AssetManagementViewModel()
        {
            Companies = new List<Company>();
            Dept = new List<Departement>();
            Locations = new List<Location>();
            Cities = new List<City>();
            AssetHistory = new List<Asset>();
            StatusList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Return", Text = "Return" },
                new SelectListItem { Value = "Borrowing", Text = "Borrowing" },
                new SelectListItem { Value = "Service", Text = "Service" },
                new SelectListItem { Value = "Ready", Text = "Ready" },
                new SelectListItem { Value = "Assign", Text = "Assign" },
                new SelectListItem { Value = "Write Off", Text = "Write Off" }
            };

        }



        public List<SelectListItem> GetCompanyListItem()
        {
            return Companies
                .Where(c => c.Is_Deleted != true)
                .Select(c => new SelectListItem
                {
                    Value = c.Company_Code,
                    Text = c.Company_Code
                })
                .ToList();
        }
        public SelectList GetDepartementListItem()
        {
            return new SelectList(Dept, "Departement_Code", "Departement_Name");
        }
        public SelectList GetCityListItem()
        {
            return new SelectList(Cities, "City_Id", "City_Name");
        }
        public SelectList GetLocationListItem()
        {
            return new SelectList(Locations, "Location_Code", "Location_Name");
        }
        public SelectList GetStatusListItem()
        {
            return new SelectList(StatusList, "Value", "Text");
        }
    }
}