using System;
using System.Collections.Generic;
using System.IO;
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
using System.Reflection;
using Sharktooth;

namespace Sniffer_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Scanner Scanner { get; } = new Scanner();
        public string Version { get => "v1.0"; }
        public bool IsRunning { get; set; } = false;

        public MainWindow()
        {
            InitializeComponent();
            //this.DataContext = this.Scanner;
            this.DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Scanner.ReadDumpFile(@"C:\Users\Cisco\Documents\WireShark\PCAP\GHTV_Booting.pcap");
            //Scanner.Setup();

            //Scanner.AddDevice(@"C:\Users\Cisco\Documents\WireShark\PCAP\GHTV_Trying.pcap");

            // Output files to relative directory
            var exeDirectory = GetExeDirectory();
            Scanner.ManifestPath = Path.Combine(exeDirectory, @"RIPPED\manifest.json");
            Scanner.OutputDirectory = Path.Combine(exeDirectory, @"RIPPED\");
        }

        private string GetExeDirectory()
        {
            var dllPath = Assembly.GetAssembly(typeof(Scanner)).Location;
            return Path.GetDirectoryName(dllPath);
        }

        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            // Hides the stupid overflow arrow
            // Source: http://stackoverflow.com/questions/4662428/how-to-hide-arrow-on-right-side-of-a-toolbar

            ToolBar toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }

            var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
            if (mainPanelBorder != null)
            {
                mainPanelBorder.Margin = new Thickness(0);
            }
        }

        private async void Button_StartScan_Click(object sender, RoutedEventArgs e)
        {
            if (IsRunning)
            {
                // Stop scan
                IsRunning = false;
                Button_StartScan.Content = "Start Scan";
                await Scanner.StopAsync();
                return;
            }

            IsRunning = true;
            Button_StartScan.Content = "Stop Scan";

            await Scanner.StartAsync();
        }
    }
}
