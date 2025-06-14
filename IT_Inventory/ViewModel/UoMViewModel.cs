﻿using System;

namespace DBIT_Inventory.ViewModel
{
    public class UoMViewModel
    {
        public int UoM_Id { get; set; }
        public string UoM_Description { get; set; }
        public string Create_By { get; set; }
        public Nullable<System.DateTime> Create_Date { get; set; }
        public string Edit_By { get; set; }
        public Nullable<System.DateTime> Edit_Date { get; set; }
        public string Delete_By { get; set; }
        public Nullable<System.DateTime> Delete_Date { get; set; }
        public Nullable<bool> Is_Deleted { get; set; }
        public string UoM_Code { get; set; }

        public string EncodedId { get; set; }

    }
}