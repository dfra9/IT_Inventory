using System;

namespace IT_Inventory.ViewModel
{
    public class CompanyViewModel
    {
        public string Company_Code { get; set; }
        public string Company_Name { get; set; }
        public string Create_By { get; set; }
        public Nullable<System.DateTime> Create_Date { get; set; }
        public string Edit_By { get; set; }
        public Nullable<System.DateTime> Edit_Date { get; set; }
        public string Delete_By { get; set; }
        public Nullable<System.DateTime> Delete_Date { get; set; }
        public Nullable<bool> Is_Deleted { get; set; }
    }
}