using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace VectorAccelerator.Plot
{
    public static class PlotHelper
    {
        public static void QuickPlot(double[] x, double[] y, 
            Tuple<double, double> xRange = null, Tuple<double, double> yRange = null)
        {
            PlotHelper.Dispatcher.Invoke(() =>
            {
                var window = new Window()
                {
                    Width = 640,
                    Height = 480
                };
                var plotControl = new PlotControl();
                plotControl.AddLine(
                    x.Zip(y, (a, b) => new OxyPlot.DataPoint(a, b))
                    .ToArray());
                if (xRange != null)
                {
                    var xAxis = plotControl.Plot.Axes.First();
                    xAxis.Minimum = xRange.Item1;
                    xAxis.Maximum = xRange.Item2;
                }
                window.Content = plotControl;
                window.Title = "Plot Window";
                window.Show();
                window.Focus();
                window.BringIntoView();
                window.InvalidateVisual();
            });
        }

        private static Dispatcher Dispatcher
        {
            get
            {
                if (_dispatcher == null) Initialiase();
                return _dispatcher;
            }
        }

        private static void Initialiase()
        {
            var thread = new Thread(new ThreadStart(ThreadStart));
            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            _are.WaitOne();
        }

        private static void ThreadStart()
        {
            if (Application.Current == null)
            {
                var application = new Application();
                application.Startup += application_Startup;
                application.Run();
            }
            else
            {
                var newWindow = new Window();
                _dispatcher = newWindow.Dispatcher;
                _are.Set();
                Dispatcher.Run();
            }
        }

        static void application_Startup(object sender, StartupEventArgs e)
        {
            _dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
            _are.Set();
        }

        private static Dispatcher _dispatcher;
        private static AutoResetEvent _are = new AutoResetEvent(false);
    }
}
