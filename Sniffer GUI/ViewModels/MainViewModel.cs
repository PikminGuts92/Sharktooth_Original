using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using Sharktooth;

namespace Sniffer_GUI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public Scanner Scanner { get; } = new Scanner();
        public string Version { get => "v1.0"; }
        public bool IsRunning { get; set; } = false;

        public event PropertyChangedEventHandler PropertyChanged;
        
        public MainViewModel()
        {
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

        public async Task ToggleScanButton()
        {
            OnPropertyChanged("Requests");

            if (IsRunning)
            {
                // Stop scan
                IsRunning = false;
                OnPropertyChanged("ScanButtonText");
                await Scanner.StopAsync();
                return;
            }

            IsRunning = true;
            OnPropertyChanged("ScanButtonText");
            await Scanner.StartAsync();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (propertyName is null)
                return;

            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public string ScanButtonText => IsRunning ? "Stop Scan" : "Start Scan";
    }
}
