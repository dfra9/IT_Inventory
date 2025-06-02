using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using IT_Inventory;

namespace DBIT_Inventory.ViewModel
{
    public class AssetManagementViewModel
    {
        public long ID { get; set; }
        [Required(ErrorMessage = "No Asset is required")]
        public string No_asset { get; set; }
        [Required(ErrorMessage = "Company Code is required")]
        public string Company_Code { get; set; }
        [Required(ErrorMessage = "Company_Name is required")]
        public string Company_Name { get; set; }

        [Required(ErrorMessage = "Material Group is required")]
        public string Material_Group { get; set; }
        [Required(ErrorMessage = "Material Code is required")]
        public string Material_Code { get; set; }
        [Required(ErrorMessage = "Material Description is required")]
        public string Material_Description { get; set; }
        public int? Quantity { get; set; }
        public string UoM { get; set; }
        public DateTime? Acquisition_Date { get; set; }
        public string Acquisition_value { get; set; }
        public string No_Asset_PGA { get; set; }
        public string No_Asset_Accounting { get; set; }
        public string No_PO { get; set; }
        public string Latest_User { get; set; }
        public string Asset_Image { get; set; }

        public string Departement_Code { get; set; }

        public string Departement_Name { get; set; }

        public string Company_User { get; set; }
        public string Locations { get; set; }
        public string Location_Name { get; set; }
        public string City_Name { get; set; }
        public DateTime? Last_Check_Date { get; set; }
        public string Condition { get; set; }
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }
        [Required(ErrorMessage = "PIC is required")]
        public string PIC { get; set; }
        public string Vendor { get; set; }
        [Required(ErrorMessage = "Transaction Date is required")]
        public DateTime? Transaction_Date { get; set; }
        public string Role { get; set; }
        public string mode { get; set; }

        public string Serial_Number { get; set; }
        public string Device_Id { get; set; }
        public string Create_By { get; set; }
        public DateTime? Create_Date { get; set; }
        public string Edit_By { get; set; }
        public DateTime? Edit_Date { get; set; }
        public string Delete_By { get; set; }
        public DateTime? Delete_Date { get; set; }
        public Nullable<bool> Is_Deleted { get; set; }

        public DashboardCountsModel DashboardCounts { get; set; }
        public List<Company> Companies { get; set; }
        [Required(ErrorMessage = "Departement is required")]
        public List<Departement> Dept { get; set; }
        [Required(ErrorMessage = "Location is required")]
        public List<Location> LocationsList { get; set; }
        [Required(ErrorMessage = "City is required")]
        public List<City> Cities { get; set; }
        public List<SelectListItem> StatusList { get; set; }

        public List<Departement> Roles { get; set; }

        public List<UoM> UoMList { get; set; }
        public List<Material_Group> MaterialGroup { get; set; }

        public List<Material_Code> Material_Code1 { get; set; }

        public List<Asset_History> AssetHistory { get; set; }

        public List<Asset> AssetList { get; set; }

        public List<Asset> DashHistory { get; set; }

        public int TotalAssets { get; set; }
        public int AvailableAssets { get; set; }
        public int AssetInUse { get; set; }
        public int AssetInService { get; set; }
        public AssetManagementViewModel()
        {
            Companies = new List<Company>();
            Dept = new List<Departement>();
            LocationsList = new List<Location>();
            Cities = new List<City>();
            AssetHistory = new List<Asset_History>();
            AssetList = new List<Asset>();
            MaterialGroup = new List<Material_Group>();
            Material_Code1 = new List<Material_Code>();
            UoMList = new List<UoM>();
            Roles = new List<Departement>();
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

        public class DashboardCountsModel
        {
            public int TotalAssets { get; set; }
            public int AvailableAssets { get; set; }
            public int AssetsInUse { get; set; }
            public int AssetsInMaintenance { get; set; }
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

        public SelectList GetLocationListItemByCity(List<Location> locations, string selectedCity)
        {
            var filterLocations = locations.Where(l => l.City_Name == selectedCity).ToList();
            return new SelectList(filterLocations, "Location_Code", "Location_Name");
        }
        public SelectList GetDepartementListItem()
        {
            return new SelectList(Dept, "Departement_Code", "Departement_Code");
        }
        public IEnumerable<SelectListItem> GetCityListItem()
        {
            var items = new List<SelectListItem>();
            if (Cities != null)
            {
                items = Cities.Select(c => new SelectListItem
                {
                    Value = c.City_Name,
                    Text = c.City_Name
                }).ToList();
            }
            return items;
        }
        public SelectList GetLocationListItem()
        {
            return new SelectList(LocationsList ?? new List<Location>(), "Location_Code", "Location_Name");
        }
        public SelectList GetStatusListItem()
        {
            return new SelectList(StatusList, "Value", "Text");
        }

        public SelectList GetMaterialGroupListItem()
        {
            var materialGroups = MaterialGroup ?? new List<Material_Group>();


            var selectListItems = materialGroups
                .Select(m => new SelectListItem
                {
                    Value = m.Material_Group1,
                    Text = m.Material_Group1
                })
                .ToList();


            selectListItems.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "Select Material Group"
            });

            return new SelectList(selectListItems, "Value", "Text");
        }

        public SelectList GetMaterialCodeListItem()
        {
            return new SelectList(Material_Code1, "Material_Code1", "Material_Code1");
        }

        public SelectList GetUoMListItem()
        {
            return new SelectList(UoMList, "UoM_Code", "UoM_Code");
        }

        public SelectList GetRoleListItem()
        {
            return new SelectList(Roles, "Role", "Role");
        }

    }
}