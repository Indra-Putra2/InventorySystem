using InventorySystem.BaseClass;
using InventorySystem.Interface;
using InventorySystem.Model;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using System.Collections.ObjectModel;

namespace InventorySystem.ViewModel.MainWindowViewModel
{
    public class DashboardViewModel : ViewModelBase
    {
        private bool _isMaximized;
        public bool IsMaximized
        {
            get { return _isMaximized; }
            set 
            { 
                _isMaximized = value;
                if (_isMaximized)
                {
                    Legend1 = LegendPosition.Right;
                }
                else
                {
                    Legend1 = LegendPosition.Hidden;
                }
            }
        }

        private IDatabaseService _database;
        public ISeries[] Series1 { get; set; }
        public Axis[] XAxes1 { get; set; }
        public Axis[] YAxes1 { get; set; }

        private LegendPosition _legend1;

        public LegendPosition Legend1
        {
            get { return _legend1; }
            set 
            { 
                _legend1 = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<DetailItem> DashboardStats { get; set; }
        public DashboardViewModel(IDatabaseService database)
        {
            _database = database;

            var datas = _database.GetRamDatas();
            var dataNonZero = datas.Where(i => i.Price > 0);
            var avgSpeed = GetAverage(datas, i => i.MemorySpeed);
            var avgCapacity = GetAverage(datas, i => i.TotalCapacity);
            var avgRating = GetAverage(datas, i => i.Rating);
            var avgPrice = GetAverage(datas, i => (double)i.Price);

            decimal best = datas
                .Where(i => i.PricePerGB > 0)
                .Select(i => i.PricePerGB)
                .DefaultIfEmpty()
                .Min();

            DashboardStats = new ObservableCollection<DetailItem>
            {
                new DetailItem { Label = "Average Price", Value = $"${avgPrice}" },
                new DetailItem { Label = "Average Speed", Value = $"{avgSpeed} MHz" },
                new DetailItem { Label = "Average Capacity (GB)", Value = $"{avgCapacity}GB" },
                new DetailItem { Label = "Average Rating", Value = $"{avgRating} Rating" },
                new DetailItem { Label = "Best Value RAM", Value = $"${best} Per GB"}
            };

            // 1. Group the data by Brand
            var brandedGroups = dataNonZero.GroupBy(i => i.Brand);

            // 2. Create a list to hold our series
            var seriesList = new List<ISeries>();

            foreach (var group in brandedGroups)
            {
                // Create a series for this specific brand
                var brandSeries = new ScatterSeries<WeightedPoint>
                {
                    Name = group.Key, // This shows the Brand name in the legend
                    Values = group.Select(item => new WeightedPoint(
                        item.MemorySpeed,
                        (double)item.PricePerGB,
                        item.TotalCapacity
                    )).ToArray(),
                    GeometrySize = 50,
                    YToolTipLabelFormatter = point =>
                        $"Speed: {point.Model.X} MHz{Environment.NewLine}" +
                        $"Price: ${point.Model.Y}/GB{Environment.NewLine}" +
                        $"Capacity: {point.Model.Weight} GB"
                };

                seriesList.Add(brandSeries);
            }

            // 3. Assign the list to your Series property
            Series1 = seriesList.ToArray();
            XAxes1 = new Axis[]
            {
                new Axis
                {
                    Name = "Memory Speed"
                }
            };

            YAxes1 = new Axis[]
            {
                new Axis
                {
                    Name = "Price Per GB"
                }
            };
        }

        private string GetAverage<T>(IEnumerable<T> source, Func<T, double> selector)
        {
            var filtered = source.Select(selector).Where(value => value > 0).ToList();
            return filtered.Any() ? double.Round(filtered.Average(), 2).ToString() : "0";
        }
    }
}