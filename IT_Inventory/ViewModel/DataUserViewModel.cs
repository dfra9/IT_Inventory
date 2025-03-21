using System;

namespace IT_Inventory.ViewModel
{
    public class DataUserViewModel
    {
        public int User_Id { get; set; }
        public string Username { get; set; }
        public string Departement { get; set; }
        public string City { get; set; }
        public string Location { get; set; }
        public DateTime? LastLogin { get; set; }

        public string Role { get; set; }
        public DateTime? LastPasswordChange { get; set; }
        public bool IsAdmin { get; set; }
    }
}