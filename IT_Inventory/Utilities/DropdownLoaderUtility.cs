using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using IT_Inventory.Models;

namespace IT_Inventory.Utilities
{
    public static class DropdownLoaderUtility
    {
        public static void LoadAllDropdownData(DBIT_Inventory db, dynamic viewBag)
        {
            viewBag.Company = db.Company.Where(c => c.Is_Deleted != true).ToList();
            viewBag.Departement = db.Departement.Where(c => c.Is_Deleted != true).ToList();
            viewBag.Location = db.Location.Where(c => c.Is_Deleted != true).ToList();
            viewBag.City = db.City.Where(c => c.Is_Deleted != true).ToList();
            viewBag.MaterialGroup = db.Material_Group.Where(c => c.Is_Deleted != true).ToList();
            viewBag.Material_Code1 = db.Material_Code.Where(c => c.Is_Deleted != true).ToList();
            viewBag.UoMList = db.UoM.Where(c => c.Is_Deleted != true).ToList();
            viewBag.Role = db.Departement.Where(c => c.Is_Deleted != true).ToList();

            viewBag.Status = new List<SelectListItem>
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


        public static List<SelectListItem> GetStatusDropdown(string selectedValue = "")
        {
            var items = new List<SelectListItem>
            {
                new SelectListItem { Text = "Select Status", Value = "" },
                new SelectListItem { Text = "Return", Value = "Return" },
                new SelectListItem { Text = "Borrowing", Value = "Borrowing" },
                new SelectListItem { Text = "Service", Value = "Service" },
                new SelectListItem { Text = "Ready", Value = "Ready" },
                new SelectListItem { Text = "Assign", Value = "Assign" },
                new SelectListItem { Text = "Write Off", Value = "Write Off" }
            };

            if (!string.IsNullOrEmpty(selectedValue))
            {
                var selected = items.FirstOrDefault(i => i.Value == selectedValue);
                if (selected != null)
                {
                    selected.Selected = true;
                }
            }

            return items;
        }


    }
}