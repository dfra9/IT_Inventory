using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IT_Inventory.ViewModel
{
    public class MaterialCodeViewModel
    {

        public int Material_CodeId { get; set; }
        [Required(ErrorMessage = "Material Code is required")]
        public string Material_Code1 { get; set; }
        [Required(ErrorMessage = "Material Description is required")]
        public string Material_Description { get; set; }
        [Required(ErrorMessage = "Material Group is required")]

        public string Material_Group { get; set; }

        public string EncodedId { get; set; }


        public string Create_By { get; set; }
        public Nullable<System.DateTime> Create_Date { get; set; }
        public string Edit_By { get; set; }
        public Nullable<System.DateTime> Edit_Date { get; set; }
        public string Delete_By { get; set; }
        public Nullable<System.DateTime> Delete_Date { get; set; }
        public Nullable<bool> Is_Deleted { get; set; }
        public List<string> MaterialGroupList { get; set; }

        public string No_Asset_PGA { get; set; }
        public string No_Asset_Accounting { get; set; }
        public string No_PO { get; set; }


    }
}