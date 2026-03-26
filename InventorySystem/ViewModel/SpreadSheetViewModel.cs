using InventorySystem.Model;

namespace InventorySystem.ViewModel
{
    public class SpreadSheetViewModel
    {
        public string ItemName { get; set; }
        public List<DetailItem> SelectedItemDetails { get; set; }

        // Parameterless Constructor
        public SpreadSheetViewModel()
        {
            ItemName = "Sample DDR5 Memory";
            // This is what the Visual Studio Designer will "see"
            SelectedItemDetails = new List<DetailItem>
            {
                new DetailItem { Label = "Name", Value = "Sample DDR5 Memory" },
                new DetailItem { Label = "Brand", Value = "Corsair" },
                new DetailItem { Label = "Type", Value = "DDR5" },
                new DetailItem { Label = "Speed", Value = "6000MT/s" },
                new DetailItem { Label = "Module", Value = "2 x 16GB" },
                new DetailItem { Label = "Total Capacity", Value = "32" },
                new DetailItem { Label = "First Word Latency", Value = "12" },
                new DetailItem { Label = "CAS Latency", Value = "36" },
                new DetailItem { Label = "Color", Value = "Black" },
                new DetailItem { Label = "Price", Value = "$273.99" },
                new DetailItem { Label = "Price Per GB", Value = "8.562" },
                new DetailItem { Label = "Review Count", Value = "45" },
                new DetailItem { Label = "Rating", Value = "4.5" },
            };
        }
    }
}