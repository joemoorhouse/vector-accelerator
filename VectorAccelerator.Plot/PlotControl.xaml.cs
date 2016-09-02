using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OxyPlot;
using OxyPlot.Wpf;

namespace VectorAccelerator.Plot
{
    /// <summary>
    /// Interaction logic for PlotControl.xaml
    /// </summary>
    public partial class PlotControl : UserControl
    {
        LinearAxis _xAxis;
        
        public PlotControl()
        {
            InitializeComponent();
        }

        public void AddLine(DataPoint[] data)
        {
            var lineSeries = new LineSeries();
            lineSeries.ItemsSource = data;
            Plot.Series.Add(lineSeries);
            var x = data.Select(d => d.X);
            _xAxis = new LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom }; //, Minimum = x.Min(), Maximum = x.Max() };
            Plot.Axes.Add(_xAxis);
        }
    }
}
