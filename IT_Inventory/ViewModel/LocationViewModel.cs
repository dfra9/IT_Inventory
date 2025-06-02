using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using IT_Inventory;

namespace DBIT_Inventory.ViewModel
{
    public class LocationViewModel
    {
        public string Location_Code { get; set; }
        public string Location_Name { get; set; }
        public string Location_Description { get; set; }
        public string City_Name { get; set; }
        public SelectList Cities { get; set; }
        public string Create_By { get; set; }
        public Nullable<System.DateTime> Create_Date { get; set; }
        public string Edit_By { get; set; }
        public Nullable<System.DateTime> Edit_Date { get; set; }
        public string Delete_By { get; set; }
        public Nullable<System.DateTime> Delete_Date { get; set; }
        public Nullable<bool> Is_Deleted { get; set; }
        public SelectList GetCityListItem(List<City> cities)
        {
            return new SelectList(cities, "City_Name", "City_Name", City_Name);
        }

        public SelectList GetLocationsByCityName(List<Location> locations, string cityName)
        {
            var filteredLocations = locations
                .Where(l => l.City_Name == cityName && l.Is_Deleted != true)
                .ToList();

            return new SelectList(filteredLocations, "Location_Code", "Location_Name");
        }

    }
}