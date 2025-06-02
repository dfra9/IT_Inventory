using System;

namespace IT_Inventory.ViewModel
{
    public class DepartementViewModel
    {
        public string Departement_Code { get; set; }
        public string Departement_Name { get; set; }
        public string Create_By { get; set; }
        public Nullable<System.DateTime> Create_Date { get; set; }
        public string Edit_By { get; set; }
        public Nullable<System.DateTime> Edit_Date { get; set; }
        public string Delete_By { get; set; }
        public Nullable<System.DateTime> Delete_Date { get; set; }
        public Nullable<bool> Is_Deleted { get; set; }
        public string Role { get; set; }
    }
}