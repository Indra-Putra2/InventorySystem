using InventorySystem.BaseClass;
using InventorySystem.Interface;
using InventorySystem.Model;
using InventorySystem.Services;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Windows;

namespace InventorySystem.ViewModel.MainWindowViewModel
{
    public class DashboardViewModel : ViewModelBase
    {
        private string _searchValue = string.Empty;
        public string SearchValue
        {
            get { return _searchValue; }
            set
            {
                if (_searchValue == value) return;

                _searchValue = value;
                OnPropertyChanged();
                if (string.IsNullOrEmpty(_searchValue))
                {
                    Search();
                }
            }
        }
        public RelayCommand SearchCommand => new RelayCommand(execute => Search());

        private bool _isMaximized;
        public bool IsMaximized
        {
            get { return _isMaximized; }
            set
            {
                _isMaximized = value;
                if (_isMaximized)
                {
                    ScatterLegend = LegendPosition.Right;
                }
                else
                {
                    ScatterLegend = LegendPosition.Hidden;
                }
            }
        }

        private IDatabaseService _database;
        //Scatter Plot//
        private ISeries[] _scatterSeries;
        public ISeries[] ScatterSeries
        {
            get { return _scatterSeries; }
            set
            {
                _scatterSeries = value;
                OnPropertyChanged();
            }
        }
        public Axis[] ScatterAxisX { get; set; }
        public Axis[] ScatterAxisY { get; set; }
        private LegendPosition _scatterLegend;
        public LegendPosition ScatterLegend
        {
            get { return _scatterLegend; }
            set
            {
                _scatterLegend = value;
                OnPropertyChanged();
            }
        }

        private ISeries[] _barPrice;
        public ISeries[] BarPrice
        {
            get { return _barPrice; }
            set { _barPrice = value; OnPropertyChanged(); }
        }
        public Axis[] BarPriceX { get; set; }
        public Axis[] BarPriceY { get; set; }

        private ISeries[] _barPricePerGB;
        public ISeries[] BarPricePerGB
        {
            get { return _barPricePerGB; }
            set { _barPricePerGB = value; OnPropertyChanged(); }
        }
        public Axis[] BarPerGBX { get; set; }
        public Axis[] BarPerGBY { get; set; }

        private ISeries[] _barOverallComparison;
        public ISeries[] BarOverallComparison
        {
            get { return _barOverallComparison; }
            set { _barOverallComparison = value; OnPropertyChanged(); }
        }
        public Axis[] BarOverallX { get; set; }

        private ISeries[] _barCapacityDistribution;
        public ISeries[] BarCapacityDistribution
        {
            get { return _barCapacityDistribution; }
            set { _barCapacityDistribution = value; OnPropertyChanged(); }
        }
        public Axis[] BarcapacityY { get; set; }
        public Axis[] BarcapacityX { get; set; }
        public ObservableCollection<DetailItem> DashboardStats { get; set; }
        public DashboardViewModel(IDatabaseService database)
        {
            _database = database;

            var datas = _database.GetRamDatas();
            var dataNonZero = datas.Where(i => i.Price > 0);

            BuildDashboardStats(datas);

            var brandedGroups = dataNonZero.GroupBy(i => i.Brand);
            var capacityGroups = dataNonZero.GroupBy(i => i.TotalCapacity).OrderBy(g => g.Key).ToList();

            ScatterSeries = BuildScatterPlot(brandedGroups);
            BarPrice = BuildBarPlotAveragePrice(brandedGroups);
            BarPricePerGB = BuildBarPlotPricePerGB(brandedGroups);
            BarOverallComparison = BuildOverallComparisonPlot(dataNonZero);
            BarCapacityDistribution = BuildBarCapacityDistribution(capacityGroups);

            BuildAllLabel(dataNonZero, brandedGroups, capacityGroups);
        }
        private string GetAverage<T>(IEnumerable<T> source, Func<T, double> selector)
        {
            var filtered = source.Select(selector).Where(value => value > 0).ToList();
            return filtered.Any() ? double.Round(filtered.Average(), 2).ToString() : "0";
        }

        private void Search()
        {
            try
            {
                var result = _database.SearchFromTable<RamData>("Products", SearchValue);
                var NonZero = result.Where(i => i.Price > 0);
                var brandGroup = NonZero.GroupBy(i => i.Brand);
                var capacityGroups = NonZero.GroupBy(i => i.TotalCapacity).OrderBy(g => g.Key).ToList();

                ScatterSeries = BuildScatterPlot(brandGroup);
                BarPrice = BuildBarPlotAveragePrice(brandGroup);
                BarPricePerGB = BuildBarPlotPricePerGB(brandGroup);
                BarOverallComparison = BuildOverallComparisonPlot(NonZero);
                BarCapacityDistribution = BuildBarCapacityDistribution(capacityGroups);

                BuildAllLabel(NonZero, brandGroup, capacityGroups);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Search Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BuildDashboardStats(List<RamData> datas)
        {
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
        }
        private ISeries[] BuildScatterPlot(IEnumerable<IGrouping<string, RamData>> groups)
        {
            List<ISeries> series = new List<ISeries>();
            foreach (var group in groups)
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
                    GeometrySize = 25,
                    YToolTipLabelFormatter = point =>
                        $"Speed: {point.Model.X} MHz{Environment.NewLine}" +
                        $"Price: ${point.Model.Y}/GB{Environment.NewLine}" +
                        $"Capacity: {point.Model.Weight} GB",
                };

                series.Add(brandSeries);
            }
            return series.ToArray();
        }
        private ISeries[] BuildBarPlotAveragePrice(IEnumerable<IGrouping<string, RamData>> groups)
        {
            var ordered = groups
                .Select(g => (g.Key, Avg: g.Average(x => (double)x.Price)))
                .OrderBy(x => x.Avg)
                .ToList();

            return new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Name = "Average Price",
                    Values = ordered.Select(i=>i.Avg).ToArray(),
                }
            };
        }
        private ISeries[] BuildBarPlotPricePerGB(IEnumerable<IGrouping<string, RamData>> groups)
        {
            var orderedGB = groups
                .Select(g => (g.Key, Avg: g.Average(x => (double)x.Price)))
                .OrderBy(x => x.Avg)
                .ToList();

            return new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Name = "Price Per GB",
                    Values = orderedGB.Select(i=>i.Avg).ToArray(),
                },
            };
        }
        private ISeries[] BuildOverallComparisonPlot(IEnumerable<RamData> allData)
        {
            // 1. Filter your data into two pools
            var ddr4Pool = allData.Where(x => x.MemoryType == "DDR4").ToList();
            var ddr5Pool = allData.Where(x => x.MemoryType == "DDR5").ToList();

            // 2. Calculate the 3 metrics for DDR4
            double ddr4Speed = ddr4Pool.Any() ? ddr4Pool.Average(x => (double)x.MemorySpeed) : 0;
            double ddr4Price = ddr4Pool.Any() ? ddr4Pool.Average(x => (double)x.Price) : 0;
            double ddr4Gb = ddr4Pool.Any() ? ddr4Pool.Average(x => (double)x.PricePerGB) : 0;

            // 3. Calculate the 3 metrics for DDR5
            double ddr5Speed = ddr5Pool.Any() ? ddr5Pool.Average(x => (double)x.MemorySpeed) : 0;
            double ddr5Price = ddr5Pool.Any() ? ddr5Pool.Average(x => (double)x.Price) : 0;
            double ddr5Gb = ddr5Pool.Any() ? ddr5Pool.Average(x => (double)x.PricePerGB) : 0;

            return new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Name = "DDR4",
                    // The order here MUST match your X-Axis labels!
                    Values = new double[] { ddr4Speed, ddr4Price, ddr4Gb },
                },
                new ColumnSeries<double>
                {
                    Name = "DDR5",
                    Values = new double[] { ddr5Speed, ddr5Price, ddr5Gb }
                }
            };
        }
        private ISeries[] BuildBarCapacityDistribution(List<IGrouping<double, RamData>> groups)
        {
            return new ISeries[]
            {
                new RowSeries<int>
                {
                    Name = "Distribution",
                    Values = groups.Select(g => g.Count()).ToArray(),

                    YToolTipLabelFormatter = point =>
                    {
                        var capacity = groups[point.Index].Key;
                        return $"{capacity} GB{Environment.NewLine}" +
                        $"{point.Coordinate.PrimaryValue} items";
                    }
                }
            };
        }
        private (Axis, Axis) BuildAxis(string Xname, string Yname, string[] labelsX = null, string[] labelsY = null, int labelrotY = 0, int LabelrotX = 0)
        {
            return (
                new Axis
                {
                    Name = Xname,
                    Labels = labelsX,
                    LabelsRotation = LabelrotX,
                },
                new Axis
                {
                    Name = Yname,
                    Labels = labelsY,
                    LabelsRotation = labelrotY,
                });
        }
        private void BuildAllLabel(IEnumerable<RamData> dataNonZero, IEnumerable<IGrouping<string, RamData>> brandedGroups, List<IGrouping<double, RamData>> capacityGroups)
        {
            var (ScatterX, ScatterY) = BuildAxis("MemorySpeed", "Price Per GB");
            ScatterAxisX = new Axis[] { ScatterX };
            ScatterAxisY = new Axis[] { ScatterY };

            var (PriceX, PriceY) = BuildAxis("Brand", "Average Price", labelsX: brandedGroups.Select(i => i.Key).ToArray());
            BarPriceX = new Axis[] { PriceX };
            BarPriceY = new Axis[] { PriceY };

            var (PerGBX, PerGBY) = BuildAxis("Brand", "Price Per GB", labelsX: brandedGroups.Select(i => i.Key).ToArray());
            BarPerGBX = new Axis[] { PerGBX };
            BarPerGBY = new Axis[] { PerGBY };

            BarOverallX = new Axis[]
            {
                new Axis
                {
                    Labels = new string[] { "Speed", "Price", "Per GB" },
                    LabelsRotation = 0,
                    SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)),
                    SeparatorsAtCenter = false,
                    TicksPaint = new SolidColorPaint(new SKColor(35, 35, 35)),
                    TicksAtCenter = true,
                }
            };
            var (BarcapX, BarcapY) = BuildAxis("Amount", "Total Capacity", labelsY: capacityGroups.Select(g => g.Key.ToString()).ToArray());
            BarcapacityX = new Axis[] { BarcapX };
            BarcapacityY = new Axis[] { BarcapY };

            OnPropertyChanged(nameof(BarPriceX));
            OnPropertyChanged(nameof(BarPriceY));

            OnPropertyChanged(nameof(BarPerGBX));
            OnPropertyChanged(nameof(BarPerGBY));

            OnPropertyChanged(nameof(BarcapacityX));
            OnPropertyChanged(nameof(BarcapacityY));
        }
    }
}